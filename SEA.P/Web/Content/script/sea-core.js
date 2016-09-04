
var Utilities = (function () {
    var regexpGroupId      = /^{.*}$/i,
        metricPrefixes     = ["p", "n", "μ", "m", "", "k", "M", "G", "T"],
        metricFactor       = [1E-12, 1E-9, 1E-6, 1E-3, 1, 1E3, 1E6, 1E9, 1E12],
        metricFactorLength = metricFactor.length;

    return {
        normalizeNumeric: function (v, mi, ma) {

            switch (($.type(mi) === "number" ? 1 : 0) + ($.type(ma) === "number" ? 2 : 0)) {
                case 1: return v < mi ? mi : v;
                case 2: return v > ma ? ma : v;
                case 3: return v < mi ? mi : (v > ma ? ma : v);
                default: return v;
            }
        },
        formattedValueInMetricPrefix: function (value, base, unitPrefix, roundValue) {

            if ((typeof value) !== "number")
                return value.toString();

            if (value === 0)
                return value.toString() + unitPrefix;

            var absValue = Math.abs(value), i = 1;

            for (; i < metricFactorLength; ++i)
                if (absValue < metricFactor[i])
                    break;

            --i;
            value /= metricFactor[i];
            return (roundValue ? Math.round(value) : (Math.round(value * 100) / 100)).toString() + " " + metricPrefixes[Math.clamp(i - base, 0, metricFactorLength)] + unitPrefix;
        },
        isGroupId: function (eId) {

            return regexpGroupId.test(eId);
        },
        tryStringifyJSON: function (value) {

            if (!value)
                return "";
            try {
                return JSON.stringify(value);
            }
            catch (err) {
                return "";
            }
        },
        tryParseJSON: function (value) {

            if (value === undefined || value === null)
                return null;
            try {
                return jQuery.parseJSON(value);
            }
            catch (err) {
                return null;
            }
        },
    }
}());

var Hub = (function () {
    var connectionId,
        hub          = $.connection.hub,
        seaHub       = $.connection.seaHub,
		online       = false,
		statusString = ["offline", "online", "standby"],
        algorithms,
        propertyValueTracking,
		self = {
		    online: function () { return online; },
		    start: function () {

		        return hub.start().then(function () {

		            online = true;
		            seaHub.server.connectToGameSession();
		            self.Callbacks.onChangeHubStatus.fire(statusString[1]);
		        }, function () {

		            online = false;
		            self.Callbacks.onChangeHubStatus.fire(statusString[0]);
		        }).promise();
		    },
		    Callbacks: {
		        onChangeSessionStatus: $.Callbacks("unique memory"),
		        onChangeHubStatus    : $.Callbacks("unique memory")
		    },
		    Storage: {
		        World: {
		            get     : function (id)  { return seaHub.server.worldGetAsync(id); },
		            getAll  : function ()    { return seaHub.server.worldGetAllAsync(); },
		            getWhere: function (eId) { return seaHub.server.worldGetWhereAsync(eId); },
		            add     : function (obj) { return seaHub.server.worldAddAsync(obj); },
		            addArray: function (arr) { return seaHub.server.worldAddArrayAsync(arr); },
		            update  : function (obj) { return seaHub.server.worldUpdateAsync(obj); },
		            delete  : function (id)  { return seaHub.server.worldDeleteAsync(id); }
		        },
		        Grid: {
		            get     : function (id)       { return seaHub.server.gridGetAsync(pId, id); },
		            getAll  : function (pId)      { return seaHub.server.gridGetAllAsync(pId); },
		            getWhere: function (pId, eId) { return seaHub.server.gridGetWhereAsync(pId, eId); },
		            add     : function (obj)      { return seaHub.server.gridAddAsync(obj); },
		            addArray: function (arr)      { return seaHub.server.gridAddArrayAsync(arr); },
		            update  : function (obj)      { return seaHub.server.gridUpdateAsync(obj); },
		            delete  : function (id)       { return seaHub.server.gridDeleteAsync(id); }
		        },
		        Control: {
		            get     : function (id)       { return seaHub.server.controlGetAsync(pId, id); },
		            getAll  : function (pId)      { return seaHub.server.controlGetAllAsync(pId); },
		            getWhere: function (pId, eId) { return seaHub.server.controlGetWhereAsync(pId, eId); },
		            add     : function (obj)      { return seaHub.server.controlAddAsync(obj); },
		            addArray: function (arr)      { return seaHub.server.controlAddArrayAsync(arr); },
		            update  : function (obj)      { return seaHub.server.controlUpdateAsync(obj); },
		            delete  : function (id)       { return seaHub.server.controlDeleteAsync(id); }
		        },
		        ControlSettings: {
		            keys  : function (pId)      { return seaHub.server.controlSettingsKeysAsync(pId); },
		            save  : function (/*(pId,key,value) | (pId,key,value)*/) { return seaHub.server.controlSettingsSaveObjectAsync(arguments.length < 2 ? { pId: arguments[0].pId, key: arguments[0].key, value: Utilities.tryStringifyJSON(arguments[0].value || "") } : { pId: arguments[0], key: arguments[1], value: Utilities.tryStringifyJSON(arguments[2] || "") }); },
		            load  : function (pId, key) { return seaHub.server.controlSettingsLoadAsync(pId, key).pipe(Utilities.tryParseJSON); },
		            delete: function (pId, key) { return seaHub.server.controlSettingsDeleteAsync(pId, key); }
		        },
		        Data: {
		            keys  : function ()           { return seaHub.server.dataKeysAsync(); },
		            save  : function (key, value) { return jQuery.type(key) === "object" ? seaHub.server.dataSaveObjectAsync(key) : seaHub.server.dataSaveAsync(key, value); },
		            load  : function (key)        { return seaHub.server.dataLoadAsync(key); },
		            delete: function (key)        { return seaHub.server.dataDeleteAsync(key); }
		        }
		    },
		    Game: {
		        /*Timeout: 2sec, Max message size: 4MB */
		        transmit               : function (value /*object*/)     { return seaHub.server.transmitAsync(value); },/*send the value to other clients*/

		        getBlockProperties     : function (eId)                  { return seaHub.server.doAsync(10, Utilities.tryStringifyJSON(eId)).pipe(Utilities.tryParseJSON); },
		        getBlockPropertiesBool : function (eId)                  { return seaHub.server.doAsync(11, Utilities.tryStringifyJSON(eId)).pipe(Utilities.tryParseJSON); },
		        getBlockPropertiesFloat: function (eId)                  { return seaHub.server.doAsync(12, Utilities.tryStringifyJSON(eId)).pipe(Utilities.tryParseJSON); },
		        getBlockActons         : function (eId)                  { return seaHub.server.doAsync(13, Utilities.tryStringifyJSON(eId)).pipe(Utilities.tryParseJSON); },

		        setValueBool           : function (eId, property, value) { return seaHub.server.doAsync(31, Utilities.tryStringifyJSON({ eId: eId, propId: property, value: value })).pipe(Utilities.tryParseJSON); },
		        setValueFloat          : function (eId, property, value) { return seaHub.server.doAsync(32, Utilities.tryStringifyJSON({ eId: eId, propId: property, value: value })).pipe(Utilities.tryParseJSON); },
		        setBlockAction         : function (eId, action)          { return seaHub.server.doAsync(33, Utilities.tryStringifyJSON({ eId: eId, action: action })).pipe(Utilities.tryParseJSON); },

		        getValueBool           : function (eId, property)        { return seaHub.server.doAsync(21, Utilities.tryStringifyJSON({ eId: eId, propId: property })).pipe(Utilities.tryParseJSON); },
		        getValueFloat          : function (eId, property)        { return seaHub.server.doAsync(22, Utilities.tryStringifyJSON({ eId: eId, propId: property })).pipe(Utilities.tryParseJSON); },
		        getValueFloatMinimum   : function (eId, property)        { return seaHub.server.doAsync(23, Utilities.tryStringifyJSON({ eId: eId, propId: property })).pipe(Utilities.tryParseJSON); },
		        getValueFloatMaximum   : function (eId, property)        { return seaHub.server.doAsync(24, Utilities.tryStringifyJSON({ eId: eId, propId: property })).pipe(Utilities.tryParseJSON); },
		        Grid: {
		            getAll: function () { return seaHub.server.doAsync(1, "").pipe(Utilities.tryParseJSON); }
		        },
		        Control: {
		            getAll: function (gridId) { return seaHub.server.doAsync(2, Utilities.tryStringifyJSON(gridId)).pipe(Utilities.tryParseJSON); }
		        },
		        getGroupBlocks     : function (gridId, groupName)        { return seaHub.server.doAsync(3, Utilities.tryStringifyJSON({ gridId: gridId, groupName: groupName })).pipe(Utilities.tryParseJSON); },
		        addValueTracking   : function (eId, property, func)      { return propertyValueTracking.add(eId, property, func); },
		        removeValueTracking: function ( /*eId, property, func*/) { return propertyValueTracking.remove(eId, property, func); }
		    },
		    errorCodeString: function (code) {

		        switch (code) {
		            case -101: return "internal_error";
		            case -102: return "invalid";
		            case -103: return "exist";
		            case -104: return "not_exist";
		            default: return "unknown_error";
		        }
		    }
		};

    //Client

    algorithms = [
    function (obj) {/*Func 0 - ! RESERVED !*/

        self.Callbacks.onChangeSessionStatus.fire(statusString[0]);
    },
    function (obj) {/*Func 1 - ! RESERVED !*/

        if (obj)
            propertyValueTracking.fire(obj);

        //console.log(obj);
    }];

    propertyValueTracking = {
        _entities: {},
        _getEntityKey: function (eId) {

            return typeof (eId) === "string" ? eId : Utilities.tryStringifyJSON(eId);
        },
        _getEntityCallbacks: function (eId) {

            return this._entities[this._getEntityKey(eId)];
        },
        _getCallback: function (eId, property, create) {

            var _eId = this._getEntityKey(eId),
				callbacks = this._entities[_eId];

            if (!callbacks)
                if (create)
                    this._entities[_eId] = callbacks = {};
                else
                    return null;

            var callback = callbacks[property];

            if (!callback)
                if (create)
                    callbacks[property] = callback = { list: $.Callbacks("memory"), count: 0 };
                else
                    return null;

            return callback;
        },
        add: function (eId, property, func) {

            if ($.type(func) !== "function" || !property || !eId) return;

            var callback = this._getCallback(eId, property, true);
            if (!callback.list.has(func)) {

                callback.list.add(func);
                callback.count++;
            }
            seaHub.server.doAsync(41, Utilities.tryStringifyJSON({ connId: connectionId, eId: eId, propId: property })).pipe(Utilities.tryParseJSON);
        },
        remove: function ( /*eId, property, func*/) {

            switch (arguments.length) {
                case 3:

                    var callback = this._getCallback(arguments[0], arguments[1], false);
                    if (!callback || !callback.list.has(arguments[2]))
                        return;

                    callback.count--;
                    callback.list.remove(arguments[2]);

                    if (callback.count == 0)
                        this.remove(arguments[0], arguments[1]);

                    break;
                case 2:

                    var callbacks = this._getEntityCallbacks(arguments[0]);
                    if (callbacks)
                        delete callbacks[arguments[1]];

                    seaHub.server.doAsync(42, Utilities.tryStringifyJSON({ connId: connectionId, eId: arguments[0], propId: arguments[1] })).pipe(Utilities.tryParseJSON);
                    break;
                case 1:

                    delete _entities[this._getEntityKey(arguments[0])];

                    seaHub.server.doAsync(42, Utilities.tryStringifyJSON({ connId: connectionId, eId: arguments[0] })).pipe(Utilities.tryParseJSON);
                    break;
            }
        },
        fire: function (eId, property, value) {

            var callback;
            switch (arguments.length) {
                case 1: callback = this._getCallback(eId.eId, eId.propId, false); if (callback) callback.list.fire(eId.value); return;
                case 3: callback = this._getCallback(eId, property, false); if (callback) callback.list.fire(value); return;
            }
        }
    };

    seaHub.client.doAsync = function (id, value) {

        var func = algorithms[id];

        if (func)
            func(Utilities.tryParseJSON(value));
    };

    seaHub.client.onWorldAdd      = function (value) { };
    seaHub.client.onWorldUpdate   = function (value) { };
    seaHub.client.onWorldDelete   = function (id) { };
    seaHub.client.onGridAdd       = function (value) { };
    seaHub.client.onGridUpdate    = function (value) { };
    seaHub.client.onGridDelete    = function (id) { };
    seaHub.client.onControlAdd    = function (value) { };
    seaHub.client.onControlUpdate = function (value) { };
    seaHub.client.onControlDelete = function (id) { };

    seaHub.client.onTransmit      = function (value) {

    }

    seaHub.client.onConnectToGameSession = function (code) {

        self.Callbacks.onChangeSessionStatus.fire(statusString[code]);
    }

    seaHub.client.connect = function (id) {
        connectionId = id
    }

    seaHub.client.onDisconnected = function (id) {

    }
    seaHub.client.disconnect = function () {

        online = false;
        hub.stop()
        self.Callbacks.onChangeHubStatus.fire(statusString[0]);
        self.Callbacks.onChangeSessionStatus.fire(statusString[0]);
    }

    //Server

    //Sistem

    hub.reconnecting(function () {
        online = false;
        self.Callbacks.onChangeHubStatus.fire(statusString[2]);
    });
    hub.reconnected(function () {
        online = true;
        self.Callbacks.onChangeHubStatus.fire(statusString[1]);
        seaHub.server.connectToGameSession();
    });
    hub.disconnected(function (id) {
        online = false;
        self.Callbacks.onChangeHubStatus.fire(statusString[0]);
        self.Callbacks.onChangeSessionStatus.fire(statusString[0]);
    });

    return self;
}());