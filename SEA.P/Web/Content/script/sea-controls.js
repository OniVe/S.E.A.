
/**
  * Default controls "S.E.A. Multi-touch"
  * Copyright 2016, "OniVe" Igor Kuritsin
  **/
$.widget( "sea_ui.controlunit", {
	version: "1.0.0",
	defaultElement: "<span>",
	options: {
		pId		  : "",/*controlId*/
		eId		  : "",/*entityId*/
        title	  : "",
		draggable : false,
		editable  : false,
		locked	  : false,
		titleLabel: 0
	},
	regexpGroupId: /^{.*}$/i,
	
	lang : "sea-ui",
    css  : "sea-ui",
	group: "#noname",
	text : "#noname",
	order: 0,
	optionsView: {
	    titleLabel: { text: "#sea-ui-control_title_label", list: [{ text: "#sea-ui-control_label-hide", value: 0 }, { text: "#sea-ui-control_label-horizontal", value: 1 }, { text: "#sea-ui-control_label-vertical", value: 2 }] },
	    valueLabel: { text: "#sea-ui-control_value_label", list: [{ text: "#sea-ui-control_label-hide", value: 0 }, { text: "#sea-ui-control_label-horizontal", value: 1 }] }
	},
	validate: function ( options ){		
		return true;
	},
	_allowEvents: function ( event ){
		
		return !this.options.locked && !this.options.draggable && !this.options.editable && (event ? (event.which === 1 || event.which === 0 || event.which === undefined) : true);
	},
    
    _create: function (){
		
		/*if(this.widgetName !== "controlunit" && (!this.options.pId || !this.options.eId)) return false;*/
		
        var self = this;
		self._normalize_eId();
        $.cssFile.on(self.css).always(function (){
            
            self.element
				.attr("title", self.options.title)
				.attr("style", "position:absolute;top:0;left:0;")
                .addClass("sea-ui-control-wrapper noselect")
				.prepend("<div class='overlay' style='display:"+ (self.options.locked ? "block" : "none") +";'></div>")
                .draggable({
                    containment  : "#sea_dashboard",
                    scroll       : false,
                    snap         : ".sea-ui-control-wrapper",
                    snapMode     : "outer",
                    snapTolerance: 4,
                    stack        : ".sea-ui-control-wrapper",
                    disabled     : !self.options.draggable,
                    opacity      : .5,
                    drag: function ( event, ui ){
                        
                        if(ui.offset.left < 175 && ui.offset.top < 35)
                            ui.position.top = 35 + ui.offset.top - ui.position.top;
                    },
                    stop: function ( event, ui ){
                        
                        self.savePosition();
                    }
                })
				.disableSelection();
            
            var titleLabel = null;
            if (self.options.titleLabel > 0) {

                titleLabel = $("<div class='label' class='position:absolute;'>" + self.options.title + "</div>");
                self.titleLabelWraper = $("<div class='label-wraper".join20(self.options.titleLabel === 2 ? "vertical" : null) + " sea-text' style='position:absolute;'></div>")
                    .append(titleLabel)
                    //.append("<div class='handle'></div>")
                    .appendTo(self.element)
                    .draggable({
                        containment  : "#sea_dashboard",
                        //handle       : ".handle",
                        scroll       : false,
                        snap         : ".sea-ui-control-wrapper",
                        delay        : 500,
                        snapTolerance: 4,
                        opacity      : .5,
                        stop: function (event, ui) {

                            self.savePosition();
                        }
                    });
            }

            self.valueLabel = null;
            if ("valueLabel" in self.options && self.options.valueLabel > 0) {

                self.valueLabel = $("<div class='label' class='position:absolute;'>???</br>???</div>");
                self.valueLabelWraper = $("<div class='label-wraper".join20(self.options.valueLabel === 2 ? "vertical" : null) + " sea-text' style='position:absolute;'></div>")
                    .append(self.valueLabel)
                    //.append("<div class='handle'></div>")
                    .appendTo(self.element)
                    .draggable({
                        containment: "#sea_dashboard",
                        //handle: ".handle",
                        scroll: false,
                        snap: ".sea-ui-control-wrapper",
                        delay: 500,
                        snapTolerance: 4,
                        opacity: .5,
                        stop: function (event, ui) {

                            self.savePosition();
                        }
                    });
            }

			self.loadPosition.call(self).always(function (){
				
				if(self.options.locked) self.lock();
				
				if($.type(self._afterCreate) === "function"){

				    self._afterCreate();
				    if (titleLabel) titleLabel.attr("style", "position:absolute;margin-left:" + Math.round(17.5 - titleLabel.width() / 2) + "px;margin-top:" + Math.round(17.5 - titleLabel.height() / 2) + "px;");
				    if (self.valueLabel) self.valueLabel.attr("style", "position:absolute;margin-left:" + Math.round(17.5 - self.valueLabel.width() / 2) + "px;margin-top:" + Math.round(17.5 - self.valueLabel.height() / 2) + "px;");
                }
			});
        });
	},
	_afterCreate: function (){
		
		this.element
			.width(25)
			.height(25)
			.addClass("ui-state-error");
	},
    _setOption: function ( key, value ){
		
		switch(key){
			case "draggable": 
				
				this.options.draggable = value;
				this.element.draggable("option", "disabled", !this.options.draggable);
			return;
			case "disable":
				
			    this.element.draggable("disable");
			break;
			case "enable":
				
			    this.element.draggable("enable");
			break;
			case "locked":
				
				value ? this.lock() : this.unlock();
			return;
			case "title":
				
				this.element.attr("title", value);
			break;
		}
		
		this._super(key, value);
	},
	_normalize_eId: function (){
		
	    /* convert to object if eId is group */
	    switch($.type(this.options.eId)){
	        case "string":
	            if(Utilities.isGroupId(this.options.eId))
	                this.options.eId = Utilities.tryParseJSON(this.options.eId);
            return;
	        case "number":
	            
	            this.options.eId = "" + this.options.eId;
            return;
	    }
	},
	_eIdIsGroup: function (){

	    return $.type(this.options.eId) === "object";
	},

	unitOptions: function (){
		
		var key, options = {};
		for(key in this.optionsView)
			options[key] = this.options[key];
		
		return options;
	},
	lock: function (){
		
	    if(this.options.locked)
			return;
		
	    this.options.locked = true;
		this.element.children(".overlay").show();
	},
	unlock: function (){
		
	    if(!this.options.locked)
			return;
		
	    this.options.locked = false;
	    if(this._syncValue) this._syncValue();
		this.element.children(".overlay").hide();
	},
	draggable: function ( a ){
		this._setOption("draggable", a === undefined ? !this.options.draggable : a);
	},
	editable: function ( a ){
		this._setOption("editable", a === undefined ? !this.options.editable : a);
	},
    loadPosition: function (){
        
        var self = this;
        return Hub.Storage.ControlSettings.load(self.options.pId, $.resolution.valid ? "position" + $.resolution.string : "position").always(function ( obj ){
                
            var $parent = self.element.parent();
            if($.isPlainObject(obj) && obj.element){
                
                if(obj.element.left < 140 && obj.element.top < 35)
                    obj.element.left = 140;
                
                /*.css("z-index", pos.zIndex || "1")*/
				self.element.position({
                    of: $parent,
                    my: "left top",
                    at: (obj.element.left < 0 ? "left": "left+") + obj.element.left +
						(obj.element.top  < 0 ? " top": " top+") + obj.element.top,
                    collision: "fit fit"
				});

				if (self.titleLabelWraper && obj.titleLabel)
				    self.titleLabelWraper.position({
				        of: self.element,
				        my: "left top",
				        at: (obj.titleLabel.left < 0 ? "left" : "left+") + obj.titleLabel.left +
						    (obj.titleLabel.top < 0 ? " top" : " top+") + obj.titleLabel.top,
				        collision: "fit fit"
				    });

				if (self.valueLabelWraper && obj.valueLabel)
				    self.valueLabelWraper.position({
				        of: self.element,
				        my: "left top",
				        at: (obj.valueLabel.left < 0 ? "left" : "left+") + obj.valueLabel.left +
						    (obj.valueLabel.top < 0 ? " top" : " top+") + obj.valueLabel.top,
				        collision: "fit fit"
				    });
            }
            else{

                /*.css("z-index", "1")*/
                self.element.position({
					of: $parent,
                    collision: "fit fit"
                });
            }
        }).promise();
    },
    savePosition: function (){
        
        Hub.Storage.ControlSettings.save({
			pId: this.options.pId,
			key: $.resolution.valid ? "position" + $.resolution.string : "position",
			value: { element: this.element.position(), titleLabel: this.titleLabelWraper ? this.titleLabelWraper.position() : null, valueLabel: this.valueLabelWraper ? this.valueLabelWraper.position() : null }
		});
    },
    setValueLabel: function (value, isHTML){

        if (this.valueLabel) {

            isHTML ? this.valueLabel.html(value) : this.valueLabel.text(value);
            this.valueLabel.attr("style", "position:absolute;margin-left:" + Math.round(17.5 - this.valueLabel.width() / 2) + "px;margin-top:" + Math.round(17.5 - this.valueLabel.height() / 2) + "px;");
        }
    }
});

$.widget( "controlunit.sea_button", $.sea_ui.controlunit, {
	version: "1.0.0",
	options: {
		width : 35,
		height: 35,
		phase : false,
		action: "OnOff"
	},
	
	lang : "sea-controls",
    css  : "sea-controls",
	group: "#sea-controls-multitouch",
	text : "#sea-control-button",
	order: 1,
	optionsView: {
		width  : {text: "#width", min: 35, max: 100},
		height : {text: "#height", min: 35, max: 100},
		phase  : {text: "#sea-control-button-phase", list: [ {text: "#sea-control-button-phase-down", value: true}, {text: "#sea-control-button-phase-up", value: false} ]},
		action : {text: "#action", list: function ( response ){ if(this.controlId) Hub.Game.getBlockActons(this.controlId).always(response); }}
	},
	
	_afterCreate: function (){
		
	    if (this._eIdIsGroup()) this.options.eId.aggr = false;

		this.button = $("<div class='button touchend'></div>");
		
		this.element
			.width(this.options.width)
			.height(this.options.height)
			.append($("<div class='sea-controlunit sea-button'></div>")
				.append(this.button));
		
		this._on( this.element, {
			"touchstart .sea-button": "_touchStart",
			"touchend .sea-button": "_touchEnd"
		});
	},
	
	_do: function ( phase ){
		
		this.button.attr('class', phase ? 'button touchstart' : 'button touchend');
		if(this.options.phase === phase){
			
			Hub.Game.setBlockAction(this.options.eId, this.options.action);
		}
	},
	_touchStart: function ( event ){
		
		if(this._allowEvents(event)){

		    this._do(true);
		    return false;
        }
	},
	_touchEnd: function ( event ){
		
	    if(this._allowEvents(event)){

	        this._do(false);
	        return false;
	    }
	}
});

$.widget( "controlunit.sea_slider", $.sea_ui.controlunit, {
	version: "1.1.0",
	options: {
	    valueLabel   : 0,
        width        : 35,
		height       : 70,
		property     : "",
	    metricPrefix : 999,
		value        : 0,
		roundValue   : false,
		minValue     : 0,
		maxValue     : 100,
		defaultValue : 0,
		externalValue: 0
	},
	_minLength: 70,
	
	lang : "sea-controls",
    css  : "sea-controls",
	group: "#sea-controls-multitouch",
	text : "#sea-control-slider",
	order: 2,
	optionsView: {
		width        : {text: "#width", min: 35, max: 350},
		height       : {text: "#height", min: 35, max: 350},
		minValue     : {text: "#min"},
		maxValue     : {text: "#max"},
		defaultValue : {text: "#defaultValue"},
		roundValue   : {text: "#sea-control-slider-round"},
		metricPrefix : {text: "#sea-control-slider-metricPrefix", list: [
            {text: "#sea-control-slider-metricPrefix-disable", value: 999},
            {text: "#sea-control-slider-metricPrefix-base_1E-12", value: -4},
            {text: "#sea-control-slider-metricPrefix-base_1E-9", value: -3},
            {text: "#sea-control-slider-metricPrefix-base_1E-6", value: -2},
            {text: "#sea-control-slider-metricPrefix-base_1E-3", value: -1},
            {text: "#sea-control-slider-metricPrefix-base_1E0", value: 0},
            {text: "#sea-control-slider-metricPrefix-base_1E3", value: 1},
            {text: "#sea-control-slider-metricPrefix-base_1E6", value: 2},
            {text: "#sea-control-slider-metricPrefix-base_1E9", value: 3},
            {text: "#sea-control-slider-metricPrefix-base_1E12", value: 4}
        ]},
		property     : {text: "#property", list: function ( response ){ if(this.controlId) Hub.Game.getBlockPropertiesFloat(this.controlId).always(response); }}
	},
	
	_afterCreate: function (){
		
	    if (this._eIdIsGroup()) this.options.eId.aggr = false;

        var vertical = this.options.height >= this.options.width;
		if(this.options[vertical ? "height" : "width"] < this._minLength)
			this.options[vertical ? "height" : "width"] = this._minLength;
		this.options.value = Utilities.normalizeNumeric(this.options.value, this.options.minValue, this.options.maxValue);
		this.track = $("<div class='track'></div>");
		this.slider = $("<div class='slider touchend' style='" + (vertical ? "height:25" : "width:25") + "px;'></div>");
		this.indicator = $("<div class='indicator' style='" + (vertical ? "height:25" : "width:25") + "px;'></div>").addClass(vertical ? "vl" : "hl");
		
		this.element
			.width(this.options.width)
			.height(this.options.height)
			.append($("<div class='sea-controlunit sea-slider'></div>")
				.append(this.track
					.append(this.indicator)
					.append(this.slider)));
		
		this.settings = {
			vertical: vertical,
			maxPos  : vertical ? this.track.innerHeight() - this.slider.outerHeight() : this.track.innerWidth() - this.slider.outerWidth(),
			pos     : 0,
			touchPos: 0,
			lastTouchEnd: 0,
			posKey      : vertical ? "top" : "left",
			touchKey    : vertical ? "pageY" : "pageX",
			myPosPrefix : vertical ? "left top+" : "left+",
			myPosSuffix : vertical ? "" : " top",
		    useMetricPrefix: this.options.metricPrefix < 999
		};
		this.touchPositionUsing = $.proxy(function ( pos ){
					
			if(pos[this.settings.posKey] !== this.settings.pos){
				
				this.settings.pos = pos[this.settings.posKey];
				this.slider.css(this.settings.posKey, pos[this.settings.posKey]);
				if(this._positionToValue()){

				    this._do();
				    this._setValueLabel();
				}
			}
		}, this);
		this.usingSliderValueToPosition = $.proxy(function (pos) {
			
			this.slider.css(this.settings.posKey, pos[this.settings.posKey]);
        }, this);
		this.usingIndicatorValueToPosition = $.proxy(function (pos) {

		    //this.indicator.animate(vertical ? { top: pos[this.settings.posKey] + "px" } : { left: pos[this.settings.posKey] + "px" }, 50);
		    this.indicator.css(this.settings.posKey, pos[this.settings.posKey]);
        }, this);
		this._externalValueUpdate = $.proxy(this._externalValueUpdate, this);

		this._valueToPosition();
		this._externalValueUpdate(this.options.value);
		this._syncValue();
        
		this._on( this.element, {
			"touchstart .track": "_touchStart",
			"touchmove .track": "_touchMove",
			"touchend .track": "_touchEnd"
		});
	},
	_destroy: function (){

		Hub.Game.removeValueTracking(this.options.eId, this.options.property, this._externalValueUpdate);
	},

	_do: function (){
		
	    Hub.Game.setValueFloat(this.options.eId, this.options.property, this.options.value);
	},
    _syncValue: function (){
        
        var self = this;
        Hub.Game.getValueFloat(self.options.eId, self.options.property).then(function ( value ){
            
            if(value === null)
                return;
            
            var _value = self.settings.useMetricPrefix ? value : (self.options.roundValue ? Math.round(value) : Math.round(value * 100) / 100);
            
            if(_value > self.options.maxValue)
                _value = self.options.maxValue;
            else if(_value < self.options.minValue)
                _value = self.options.minValue;
            
            self.options.value = _value;
            if(_value !== value)
                self._do();
            
            self._externalValueUpdate(_value);
            self._valueToPosition();
        });

        Hub.Game.addValueTracking(this.options.eId, this.options.property, this._externalValueUpdate);
    },
    _externalValueUpdate: function (value) {

        if (value === undefined)
            return;

        this.options.externalValue = this.settings.useMetricPrefix ? value : (this.options.roundValue ? Math.round(value) : Math.round(value * 100) / 100);

    	var pos;
    	if (this.options.externalValue <= this.options.minValue)
    		pos = this.settings.vertical ? this.settings.maxPos : 0;
    	else if (this.options.externalValue >= this.options.maxValue)
    		pos = this.settings.vertical ? 0 : this.settings.maxPos;
    	else {

    		pos = Math.round(this.settings.vertical ?
				this.settings.maxPos * (this.options.externalValue - this.options.maxValue) / (this.options.minValue - this.options.maxValue) :
				this.settings.maxPos * (this.options.externalValue - this.options.minValue) / (this.options.maxValue - this.options.minValue));

    		if (pos > this.settings.maxPos)
    			pos = this.settings.maxPos;
    		else if (pos < 0)
    			pos = 0;
    	}

    	this.indicator.position({
    		within: this.track,
    		of: this.track,
    		at: "left top",
    		my: this.settings.myPosPrefix + pos + this.settings.myPosSuffix,
    		collision: "fit fit",
    		using: this.usingIndicatorValueToPosition
    	});

    	this._setValueLabel();
    },
	_touchStart: function ( event ){
		
	    if(this._allowEvents(event)){

	        this.slider.attr('class', 'slider touchstart');
	        this.settings.startPos = this.slider.position()[this.settings.posKey] - event.originalEvent.targetTouches[0][this.settings.touchKey];

	        return false;
	    }
	},
	_touchMove: function ( event ){
		
		if(this._allowEvents(event)){

		    this.slider.position({
		        within: this.track,
		        of: this.track,
		        at: "left top",
		        my: this.settings.myPosPrefix + (this.settings.startPos + event.originalEvent.targetTouches[0][this.settings.touchKey]) + this.settings.myPosSuffix,
		        collision: "fit fit",
		        using: this.touchPositionUsing
		    });

		    return false;
		}
	},
	_touchEnd: function ( event ){
		
		if(this._allowEvents(event)){

		    this.slider.attr('class', 'slider touchend');
		    if ((event.timeStamp - this.settings.lastTouchEnd) > 250)/* 250ms */
		        this.settings.lastTouchEnd = event.timeStamp;
		    else
		        this._doubleTap();

		    return false;
		}
	},
	_doubleTap: function (){
		
		if(this.options.value === this.options.defaultValue) return;
        
		this.options.value = this.options.defaultValue;
		this._valueToPosition();
		this._do();
		this._setValueLabel();
	},
	_positionToValue: function (){
		
		var val, oldVal = this.options.value;
		if(this.settings.pos === 0)
			val = this.settings.vertical ? this.options.maxValue : this.options.minValue;
		else if(this.settings.pos === this.settings.maxPos)
			val = this.settings.vertical ? this.options.minValue : this.options.maxValue;
		else{
			
		    val = this.settings.vertical ?
                this.options.maxValue + (this.settings.pos / this.settings.maxPos) * (this.options.minValue - this.options.maxValue) :
                this.options.minValue + (this.settings.pos / this.settings.maxPos) * (this.options.maxValue - this.options.minValue);

		    if(!this.settings.useMetricPrefix)
		        val = this.options.roundValue ? Math.round(val) : (Math.round(val * 100) / 100);
			
            if(val > this.options.maxValue)
                val = this.options.maxValue;
            else if(val < this.options.minValue)
                val = this.options.minValue;
		}
		
		this.options.value = val;
		return val !== oldVal;
	},
	_valueToPosition: function (){
		
		var pos, oldPos = this.settings.pos;
		if(this.options.value <= this.options.minValue)
			pos = this.settings.vertical ? this.settings.maxPos : 0;
		else if(this.options.value >= this.options.maxValue)
			pos = this.settings.vertical ? 0 : this.settings.maxPos;
		else{
			
			pos = Math.round(this.settings.vertical ?
                this.settings.maxPos * (this.options.value - this.options.maxValue) / (this.options.minValue - this.options.maxValue) :
                this.settings.maxPos * (this.options.value - this.options.minValue) / (this.options.maxValue - this.options.minValue));
				
            if(pos > this.settings.maxPos)
                pos = this.settings.maxPos;
            else if(pos < 0)
                pos = 0;
		}
		
		this.settings.pos = pos;
		if(pos !== oldPos){
			
			this.slider.position({
				within: this.track,
				of: this.track,
				at: "left top",
				my: this.settings.myPosPrefix + this.settings.pos + this.settings.myPosSuffix,
				collision: "fit fit",
				using: this.usingSliderValueToPosition
			});
			return true;
		}
		else
			return false;
	},
	_setValueLabel: function () {

	    this.setValueLabel(this.settings.useMetricPrefix ?
            (Utilities.formattedValueInMetricPrefix(this.options.value, this.options.metricPrefix, "", this.options.roundValue) + "</br>" + Utilities.formattedValueInMetricPrefix(this.options.externalValue, this.options.metricPrefix, "", this.options.roundValue)) :
            (this.options.value + "</br>" + this.options.externalValue), true);
	}
});

$.widget( "controlunit.sea_switch", $.sea_ui.controlunit, {
    version: "1.0.0",
	options: {
	    valueLabel: 0,
        width     : 35,
		height    : 50,
        mode      : true,
		inverse   : false,
		property  : "",
		value     : false
	},
	_minLength: 50,
	
	lang : "sea-controls",
    css  : "sea-controls",
	group: "#sea-controls-multitouch",
	text : "#sea-control-switch",
	order: 3,
	optionsView: {
		width    : {text: "#width", min: 35, max: 350},
		height   : {text: "#height", min: 35, max: 350},
        mode     : {text: "#sea-control-switch-mode" , list: [ {text: "#sea-control-switch-mode-shift", value: true}, {text: "#sea-control-switch-mode-press", value: false} ]},
		inverse  : {text: "#inverse"},
		property : {text: "#property", list: function ( response ){ if(this.controlId) Hub.Game.getBlockPropertiesBool(this.controlId).always(response); }}
	},
	
    _afterCreate: function (){
		
        if (this._eIdIsGroup()) this.options.eId.aggr = false;

		var vertical = this.options.height >= this.options.width;
		if(this.options[vertical ? "height" : "width"] < this._minLength)
			this.options[vertical ? "height" : "width"] = this._minLength;
        
		this.element
			.width(this.options.width)
			.height(this.options.height);
		
        this.track = $("<div class='track'></div>");
		this.slider = $("<div class='slider touchend' style='" + (vertical ? "height:50" : "width:50") + "%;'></div>");
		
		$("<div class='sea-controlunit sea-switch'></div>")
            .append(this.track
            .append(this.slider))
            .appendTo(this.element);
        
        this.settings = {
            vertical: vertical,
            maxPos  : vertical ? this.track.innerHeight() - this.slider.outerHeight() + 1: this.track.innerWidth() - this.slider.outerWidth() + 1,
            pos     : 0,
            startPos: 0,
            posKey      : vertical ? "top" : "left",
            touchKey    : vertical ? "pageY" : "pageX",
            myPosPrefix : vertical ? "left top+" : "left+",
            myPosSuffix : vertical ? "" : " top",
        };
        var rz = Math.ceil(this.settings.maxPos * .25);//25%
        this.settings.minReturnPos = this.settings.vertical ? this.settings.maxPos - rz : rz;
        this.settings.maxReturnPos = this.settings.vertical ? rz : this.settings.maxPos - rz;
        this.positionUsing = $.proxy(function ( pos ){
			
			if(pos[this.settings.posKey] !== this.settings.pos){
				
				this.settings.pos = pos[this.settings.posKey];
				this.slider.css(this.settings.posKey, pos[this.settings.posKey]);
			}
		}, this);
		this.valueToPositionUsing = $.proxy(function ( pos ){
			
			this.slider.css(this.settings.posKey, pos[this.settings.posKey]);
		}, this);
		
		this._externalValueUpdate = $.proxy(this._externalValueUpdate, this);
        this._valueToPosition();
        this._syncValue();
        
		if(this.options.mode)
			this._on( this.element, {
				"touchstart .track": "_touchStart",
				"touchmove .track": "_touchMove",
				"touchend .track": "_touchEnd"
			});	
		else
			this._on( this.element, {
				"touchstart .track": "_touchStart",
				"touchend .track": "_touchEnd"
			});
	},
	_do: function (){
		
		Hub.Game.setValueBool(this.options.eId, this.options.property, this.options.inverse ? !this.options.value : this.options.value);
	},
    _syncValue: function (){
        
        var self = this;
        Hub.Game.getValueBool(self.options.eId, self.options.property).then(function ( value ){
            
            if(value === null)
                return;
            
            self.options.value = value;
            self._valueToPosition();
            self.setValueLabel(self.options.inverse ? !self.options.value : self.options.value ? L("#on") : L("#off"));
        });

        Hub.Game.addValueTracking(this.options.eId, this.options.property, this._externalValueUpdate);
    },
    _externalValueUpdate: function (value) {

        if (value === undefined)
            return;

        this.options.value = value;
        this._valueToPosition();
        this.setValueLabel(this.options.inverse ? !this.options.value : this.options.value ? L("#on") : L("#off"));
    },
	_touchStart: function ( event ){
		
	    if(this._allowEvents(event)){

	        this.slider.attr('class', 'slider touchstart');
	        this.settings.startPos = this.slider.position()[this.settings.posKey] - event.originalEvent.targetTouches[0][this.settings.touchKey];
	        
	        return false;
        }
	},
	_touchMove: function ( event ){
		
		if(this._allowEvents(event)){
		
		    this.slider.position({
		        within: this.track,
		        of: this.track,
		        at: "left top",
		        my: this.settings.myPosPrefix + (this.settings.startPos + event.originalEvent.targetTouches[0][this.settings.touchKey]) + this.settings.myPosSuffix,
		        collision: "fit fit",
		        using: this.positionUsing
		    });

		    return false;
        }
	},
	_touchEnd: function ( event ){
		
		if(this._allowEvents(event)){
		    
		    this.slider.attr('class', 'slider touchend');
		    if (this._positionToValue()) {

		        this.track.attr('class', this.options.value ? 'track on' : 'track');
		        this._do();
		        this.setValueLabel(this.options.inverse ? !this.options.value : this.options.value ? L("#on") : L("#off"));
		    }

		    return false;
        }
	},
	_positionToValue: function (){
		
        var oldVal = this.options.value, val = oldVal;
        if(this.options.mode){
            
            if(oldVal){
                if(this.settings.vertical ? this.settings.pos > this.settings.maxReturnPos : this.settings.pos < this.settings.maxReturnPos)
                    val = false;
            }
            else{
                if(this.settings.vertical ? this.settings.pos < this.settings.minReturnPos : this.settings.pos > this.settings.minReturnPos)
                    val = true;
            }
        }
        else
            val = !val;
        
		this.options.value = val;
        this._valueToPosition();
		return val !== oldVal;
	},
	_valueToPosition: function (){
		
		var pos, oldPos = this.settings.pos;
		if(this.options.value)
			pos = this.settings.vertical ? 0 : this.settings.maxPos;
		else
			pos = this.settings.vertical ? this.settings.maxPos : 0;
		
		this.settings.pos = pos;
		if(pos !== oldPos){
			
            this.track.attr('class', this.options.value ? 'track on' : 'track');
			this.slider.position({
				within: this.track,
				of: this.track,
				at: "left top",
				my: this.settings.myPosPrefix + this.settings.pos + this.settings.myPosSuffix,
				collision: "fit fit",
				using: this.valueToPositionUsing
			});
            
			return true;
		}
		else
			return false;
	}
});

$.widget( "controlunit.sea_angle_controller", $.sea_ui.controlunit, {
    version: "1.0.0",
	options: {
	    diameter: 70,
	    convert : 0,
		value   : 0 /* Radian */
	},
	
	lang : "sea-controls",
    css  : "sea-controls",
	group: "#sea-controls-multitouch",
	text : "#sea-control-handwheel",
	order: 4,
	optionsView: {
	    diameter: { text: "#diameter", min: 70, max: 350 },
	    convert : { text: "#sea-control-handwheel-convert", list: [{ text: "#sea-control-handwheel-convert-pix2", value: 0 }, { text: "#sea-control-handwheel-convert-pm_pi", value: 1 }] }
		//property : {text: "#property", list: function ( response ){ if(this.controlId) Hub.Game.getBlockPropertiesFloat(this.controlId).always(response); }}
	},
    
    _afterCreate: function (){
		
        if (this._eIdIsGroup()) this.options.eId.aggr = false;

        var cxy = Math.round(this.options.diameter / 2), protractor = "<g class='protractor'>", splitStep = 15, minAngle = -180, maxAngle = 180;
        this.options.diameter = cxy * 2;
        
        if(minAngle < 0 && maxAngle > 0){
            
            protractor += "<use xlink:href='#v_split_"+ this.uuid +"'></use>";
            var i = 0;
            while(i < maxAngle){
                
                i += splitStep;
                if(i > maxAngle)
                    i = maxAngle;
                protractor += "<use xlink:href='#v_split_" + this.uuid + "' transform='rotate(" + i + "," + cxy + "," + cxy + ")'></use>";
            }
            
            i = 0;
            while(i > minAngle){
                
                i-=splitStep;
                if(i < minAngle)
                    i = minAngle;
                protractor += "<use xlink:href='#v_split_" + this.uuid + "' transform='rotate(" + i + "," + cxy + "," + cxy + ")'></use>";
            }
        }
        protractor += "</g>";

        this.settings = {
            angle: 0,
            regexpAngle: /^rotate\s*\((\-?\d*\.?\d*)/i,
            cxy: cxy,
            centerPos: { left: 0, top: 0, cosFi: 0, sinFi: 0 },
            vA: { x: 0, y: 0 },
            vB: { x: 0, y: 0 }
        };

        this.element
			.width(this.options.diameter)
			.height(this.options.diameter);
		
        this.element.append("<svg class='sea-text sea-handwheel-background' xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns='http://www.w3.org/2000/svg' height='"+ this.options.diameter +"' width='"+ this.options.diameter +"' version='1.1' xmlns:cc='http://creativecommons.org/ns#' xmlns:xlink='http://www.w3.org/1999/xlink' viewBox='0 0 "+ this.options.diameter +" "+ this.options.diameter +"' xmlns:dc='http://purl.org/dc/elements/1.1/'><circle class='cl' cx='"+ cxy +"' cy='"+ cxy +"' r='"+ (cxy - 1) +"' fill='#fff'></circle></svg>");
        this.label = $("<span class='sea-text sea-handwheel-label' style='top:" + cxy + "px;'>0</span>").appendTo(this.element);
        this.element.append("<svg class='sea-controlunit sea-handwheel' xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns='http://www.w3.org/2000/svg' height='"+ this.options.diameter +"' width='"+ this.options.diameter +"' version='1.1' xmlns:cc='http://creativecommons.org/ns#' xmlns:xlink='http://www.w3.org/1999/xlink' viewBox='0 0 "+ this.options.diameter +" "+ this.options.diameter +"' xmlns:dc='http://purl.org/dc/elements/1.1/'><def><path id='v_split_"+ this.uuid +"' stroke-width='1px' fill='none' stroke='#97afcf' stroke-linecap='round' d='M"+ cxy +",1 v5'></path></def>"+ protractor +"<g class='flywheel'><path class='arrow' d='M"+ cxy +",5 "+ (cxy - 10) +",25 "+ (cxy + 10) +",25 Z' fill='#97afcf'></path><circle class='cl' cx='"+ cxy +"' cy='"+ cxy +"' r='"+ (cxy - 1) +"' stroke-width='2px' stroke='#97afcf' fill='transparent'></circle></g></svg>");
		
        this.protractor = this.element.find(".protractor:first");
        this.flywheel = this.element.find(".flywheel:first");
        this.arrow = this.element.find(".arrow:first");
        this.circle = this.element.find(".cl:first");

        this._valueToPosition();
        this._syncValue();
        
		this._on(this.flywheel, {
			"touchstart .cl": "_touchStart",
			"touchmove .cl": "_touchMove",
			"touchend .cl": "_touchEnd"
		});
    },
    _do: function (){

        Hub.Game.setValueFloat(this.options.eId, "Virtual Angle", this.options.value < 0 ? 360 + this.options.value : this.options.value);
    },
    _syncValue: function (){

        var self = this;
        Hub.Game.getValueFloat(self.options.eId, "Virtual Angle").then(function (value){

            if (value === null)
                return;

            self.options.value = Math.round(self._anglePlusMinusPi(value) * 100) / 100;
            self.settings.angle = self.options.value * Math.PI / 180;
            self._valueToPosition();
        });
    },
    _touchStart: function ( event ){
		
		if(this._allowEvents(event)){

		    this.settings.centerPos = this.circle.offset();
		    this.settings.centerPos.left += this.settings.cxy;
		    this.settings.centerPos.top += this.settings.cxy;

		    var cosFi = Math.cos(this.settings.angle),
                sinFi = Math.sin(this.settings.angle),
                x = event.originalEvent.targetTouches[0].pageX - this.settings.centerPos.left,
                y = event.originalEvent.targetTouches[0].pageY - this.settings.centerPos.top;
		    this.settings.vA.x = x * cosFi + y * sinFi;
		    this.settings.vA.y = -x * sinFi + y * cosFi;

		    return false;
        }
	},
    _touchMove: function ( event ){
		
		if(this._allowEvents(event)){
		    
		    this.settings.vB.x = event.originalEvent.targetTouches[0].pageX - this.settings.centerPos.left;
		    this.settings.vB.y = event.originalEvent.targetTouches[0].pageY - this.settings.centerPos.top;

		    this.settings.angle = Math.atan2(this.settings.vA.x * this.settings.vB.y - this.settings.vA.y * this.settings.vB.x, this.settings.vA.x * this.settings.vB.x + this.settings.vA.y * this.settings.vB.y);
		    this.options.value = Math.round(this.settings.angle * 18000 / Math.PI) / 100;

		    this._do();
		    this._valueToPosition();

		    return false;
        }
	},
    _touchEnd: function ( event ){
		
        if(this._allowEvents(event)){

            //this.slider.attr('class', 'slider touchend');
            /*if((event.timeStamp - this.settings.lastTouchEnd) > 250)// 250ms
                this.settings.lastTouchEnd = event.timeStamp;
            else
                this._doubleTap();*/

            return false;
        }
	},
	_positionToValue: function ( event ){
		
	},
	_valueToPosition: function (){

	    this.label.text(this.options.convert === 0 ? (this.options.value < 0 ? Math.round((360.001 + this.options.value) * 100) / 100 : this.options.value) : this.options.value);
	    this._elementAngle(this.arrow, this.options.value);
	},
	_anglePlusMinusPi: function ( angle ){
	    
	    angle -= Math.floor(angle / 360) * 360;
	    if (angle > 180)
	        return angle - 360;

	    return angle;
	},
    _elementAngle: function ( element, angle ){
        
        if(angle === undefined){
            
            var res = this.settings.regexpAngle.exec(element.attr("transform"));
            if (res === null || res.length < 2)
                return 0;
            
            try{
                var val = parseFloat(res[1]);
                return isNaN(val) ? 0 : Math.round(val * 100) / 100;
            }
            catch(err){ return 0; }
        }
        else{
            
            element.attr("transform", "rotate("+ angle +","+ this.settings.cxy +","+ this.settings.cxy +")");
        }
    }
});

