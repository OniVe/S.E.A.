
/*!
 * jQuery UI Touch Punch 0.2.3
 *
 * Copyright 2011–2014, Dave Furfero
 * Dual licensed under the MIT or GPL Version 2 licenses.
 *
 * Depends:
 *  jquery.ui.widget.js
 *  jquery.ui.mouse.js
 */
!function(a){function f(a,b){if(!(a.originalEvent.touches.length>1)){a.preventDefault();var c=a.originalEvent.changedTouches[0],d=document.createEvent("MouseEvents");d.initMouseEvent(b,!0,!0,window,1,c.screenX,c.screenY,c.clientX,c.clientY,!1,!1,!1,!1,0,null),a.target.dispatchEvent(d)}}if(a.support.touch="ontouchend"in document,a.support.touch){var e,b=a.ui.mouse.prototype,c=b._mouseInit,d=b._mouseDestroy;b._touchStart=function(a){var b=this;!e&&b._mouseCapture(a.originalEvent.changedTouches[0])&&(e=!0,b._touchMoved=!1,f(a,"mouseover"),f(a,"mousemove"),f(a,"mousedown"))},b._touchMove=function(a){e&&(this._touchMoved=!0,f(a,"mousemove"))},b._touchEnd=function(a){e&&(f(a,"mouseup"),f(a,"mouseout"),this._touchMoved||f(a,"click"),e=!1)},b._mouseInit=function(){var b=this;b.element.bind({touchstart:a.proxy(b,"_touchStart"),touchmove:a.proxy(b,"_touchMove"),touchend:a.proxy(b,"_touchEnd")}),c.call(b)},b._mouseDestroy=function(){var b=this;b.element.unbind({touchstart:a.proxy(b,"_touchStart"),touchmove:a.proxy(b,"_touchMove"),touchend:a.proxy(b,"_touchEnd")}),d.call(b)}}}(jQuery);

/*
 jquery.fullscreen 1.1.4
 https://github.com/kayahr/jquery-fullscreen-plugin
 Copyright (C) 2012 Klaus Reimer <k@ailis.de>
 Licensed under the MIT license
 (See http://www.opensource.org/licenses/mit-license)
*/
function d(b){var c,a;if(!this.length)return this;c=this[0];c.ownerDocument?a=c.ownerDocument:(a=c,c=a.documentElement);if(null==b){if(!a.cancelFullScreen&&!a.webkitCancelFullScreen&&!a.mozCancelFullScreen)return null;b=!!a.fullScreen||!!a.webkitIsFullScreen||!!a.mozFullScreen;return!b?b:a.fullScreenElement||a.webkitCurrentFullScreenElement||a.mozFullScreenElement||b}b?(b=c.requestFullScreen||c.webkitRequestFullScreen||c.mozRequestFullScreen)&&b.call(c,Element.ALLOW_KEYBOARD_INPUT):(b=a.cancelFullScreen||a.webkitCancelFullScreen||a.mozCancelFullScreen)&&b.call(a);return this}jQuery.fn.fullScreen=d;jQuery.fn.toggleFullScreen=function(){return d.call(this,!d.call(this))};var e,f,g;e=document;e.webkitCancelFullScreen?(f="webkitfullscreenchange",g="webkitfullscreenerror"):e.mozCancelFullScreen?(f="mozfullscreenchange",g="mozfullscreenerror"):(f="fullscreenchange",g="fullscreenerror");jQuery(document).bind(f,function(){jQuery(document).trigger(new jQuery.Event("fullscreenchange"))});
jQuery(document).bind(g, function () { jQuery(document).trigger(new jQuery.Event("fullscreenerror")) });

/**
  * "sea_ui" Widgets
  * Copyright 2016, "OniVe" Igor Kuritsin
  * Dual licensed under the MIT and GPL licenses.
  *
  **/
$.widget( "sea_ui.dashboard", {
	version: "1.0.0",
	options: {
		uid		 : 1,
		gridId	 : "",
        draggable: false,
        editable : false,
        locked   : false,
		
		editControlUnit: $.noop,
    },
	_controlUnits: {},
	
	_create: function (){
		
		this.element
			.addClass("noselect")
			.attr("uid", this.options.uid);
		
		this.element.on("contextmenu", false);
		this._on(window, {"throttledresize": "_onResize"});
		this._changeSessionStatus = $.proxy(this._changeSessionStatus, this);
		this._changeHubStatus = $.proxy(this._changeHubStatus, this);
		Hub.Callbacks.onChangeSessionStatus.add(this._changeSessionStatus);
		Hub.Callbacks.onChangeHubStatus.add(this._changeHubStatus);
	},
	_destroy: function (){
		
	    this.element.off("contextmenu");
		this._off(window, "throttledresize");
		Hub.Callbacks.onChangeSessionStatus.remove(this._changeSessionStatus);
		Hub.Callbacks.onChangeHubStatus.remove(this._changeHubStatus);
	},
	_doControlUnits: function (){
		
		if(arguments.length === 0) return;
		var uid, name = arguments[0];
		if(arguments.length > 1){
			
			Array.prototype.shift.apply(arguments, arguments);
			for(uid in this._controlUnits) this._controlUnits[uid][name].apply(this._controlUnits[uid], arguments);
		}
		else
			for(uid in this._controlUnits) this._controlUnits[uid][name]();
	},
	_setOption: function ( key, value ){
		
		switch(key){
			case "draggable": 
				
				this._doControlUnits("option", "draggable", value);
			break;
			case "editable":
				
				if(!value)
					this._off(this.element, "click .sea-ui-control-wrapper");
				
				this._doControlUnits("option", "editable", value);
				
				if(value && this.options.editControlUnit)
					this._on(this.element, {"click .sea-ui-control-wrapper": "_onEditControlUnit"});
			break;
			case "locked":
				
				if(value)
					this._doControlUnits("lock");
				else
					this._doControlUnits("unlock");
			break;
			case "gridId":
			
				this.load(value);
			return false;
		}
		this._super(key, value);
	},

	_onResize: function ( event, resolutionChange ){
		
		this.updatePosition(resolutionChange);
	},
	_validatePosition: function ( pos ){
		
		if(pos.left < 175 && pos.top < 35)
			pos.left = 35;
		
		return pos;
	},
	_onEditControlUnit: function ( event ){
		
		this.options.editControlUnit(event, $(event.currentTarget));
	},
	_createControlUnit: function ( control, key, options ){
		
		var controlUnit, uid = control.id.toString();
		if($.controlunit.hasOwnProperty(key)){
			
			options.pId       = control.id;
			options.eId       = control.eId;
			options.title     = control.title;
			options.draggable = this.options.draggable;
			options.editable  = this.options.editable;
			options.locked    = this.options.locked;
			
			controlUnit = $.controlunit[key](options);
		}
		else{
			
			key = null;
			controlUnit = $.sea_ui.controlunit({pId: control.id, eId: control.eId, title: control.title || L("#noname")});
		}
		
		this._controlUnits[uid] = controlUnit;
		controlUnit.element
			.data({ "key": key, "uid": uid})
			.appendTo(this.element);
	},
	addControlUnit: function ( control, key, options ){
		
		if(!control) return;
		
		if(key)
			this._createControlUnit(control, key, options);
		else{
			
			var self = this;
			Hub.Storage.ControlSettings.load(control.id, "controlUnit").then(function ( obj ){
				
				if(obj)
					self._createControlUnit.call(self, control, obj.key, obj.options);
				else
					self._createControlUnit.call(self, control);
			}, function ( err ){
                
                UI.report(L("#errorGetStorageData"), err, "error");
            });
		}
	},
	deleteControlUnit: function ( uid ){
		
		if(!uid) return;
		
		if(typeof uid === "number") uid = uid.toString();
		
		var controlUnit = this._controlUnits[uid];
		if(!controlUnit) return;
		
		controlUnit.element.remove();
		delete this._controlUnits[uid];
	},
	updateControlUnit: function ( control, key, options ){
		
		if(!control) return;
		
		this.deleteControlUnit(control.id);
		this.addControlUnit(control, key, options);
	},
	
	load: function ( gridId ){
		
		if(this.options.gridId === gridId) return;
		
		this.options.gridId = gridId;
		this.element.empty();
		this._controlUnits = {};
		
		if(!gridId || !Hub.online()) return;
		
		var self = this;
		Hub.Storage.Control.getAll(gridId).then(function ( arr ){
			
			if ($.type(arr) === "array")
				for(var i = 0; i < arr.length;)
					self.addControlUnit.call(self, arr[i++]);
			
		}, function ( err ){
			
			UI.report(L("#errorGetStorageData"), err, "error");
		});
	},
	updatePosition: function ( force ){
		
		var uid, pos;
		if(force)
			this._doControlUnits("loadPosition");
		else
			for(uid in this._controlUnits){
				
				pos = this._validatePosition(this._controlUnits[uid].element.position());
				this._controlUnits[uid].element.position({
					within: this.element,
					of: this.element,
					at: "left top",
					my: (pos.left < 0 ? "left": "left+") + pos.left +
						(pos.top  < 0 ? " top": " top+") + pos.top,
					collision: "fit fit"
				});
			}
	},
	
	_updateStatus: function(){
		
	    this.options.locked = !(this.sessionOnline && this.hubOnline);
	    this._setOption("locked", this.options.locked);
	},
	_changeSessionStatus: function ( status ){
		
		switch (status){
			
			case "online" : this.sessionOnline = true; break;
			case "standby": this.sessionOnline = false; break;
			default       : this.sessionOnline = false; break;
		}
		this._updateStatus();
	},
	_changeHubStatus: function ( status ){
		
		switch (status){
			
			case "online" : this.hubOnline = true; break;
			case "standby": this.hubOnline = false; break;
			default       : this.hubOnline = false; break;
		}
		this._updateStatus();
	}
});
$.widget( "sea_ui.sidebarmenu", {
	version: "1.0.0",
	options: {},
	_buttons: {
	"sea": { /* Group 1 */
		group: 1,
		options: {
			visible: true
		},
		action: function ( button ){
			
			this.settings.open = !this.settings.open;
			if(this.settings.open){
				
				this.treeList.empty();
				this._treeListToggleSelectable(false);
				this.element.addClass("open");
				this._loadWorlds();
			}
			else{
				
				this.treeList.hide();
				this.element.removeClass("open");
			}
		}
	},
	"status": {
		group: 1,
		options: {
			visible: true,
			special: true
		},
		action: function ( button){
			
			this._buttons.sea.action.call(this, this._buttons["sea"]);
		}
	},
	"control-add": { /* or Edit*/
		group: 1,
		options: {
			floatRight: true,
			visible: false
		},
		action: function ( button, $control ){
			
			var self = this,
				isNew = !$control,
				controlKey = isNew ? "" : $control.data("key"),
				iControl = isNew || !controlKey ? null : $control[controlKey]("instance"),
				control = {
					id   : isNew ? 0 : parseInt($control.data("uid")),
					pId  : this.settings.gridId,
					eId  : isNew || !iControl ? "" : iControl.option("eId"),
					title: isNew ? "" : $control.attr("title")
				},
				gridEntityId = this.settings.gridEntityId,
				iControlUnitOptions = $.sea_ui.controlunitoptions({
					key : controlKey,
					name: "control-options",
					unitOptions: isNew || !iControl ? {} : iControl.unitOptions()
				}),
				itemfields = {};

			if (!isNew) {

			    iControlUnitOptions.property("controlId", control.eId);
			    if($.type(control.eId) === "object")
			        control.eId = Utilities.tryStringifyJSON($.extend({}, control.eId, { aggr: false }));
            }
			
			$("<div>")
			.sidebarpanel({
				title     : L(isNew ? "#add" : "#edit") + " " + L("#control").toLowerCase(),
				appendTo  : "#sea_ui",
				autoOpen  : true,
				autoRemove: true,
				class     : "addOrEditControl",
				buttons   : [{
					text : L(isNew ? "#add" : "#edit"),
					click: function ( event, element ){
						
						var that = this,
							valid = true,
							controlUnit = {
								key    : itemfields["cotrol-uid"].itemfield("value"),
								options: iControlUnitOptions.unitOptions()
							};
						control.title = itemfields["cotrol-title"].itemfield("value");
						control.eId   = itemfields["cotrol-entityid"].itemfield("value");
						
						if(!control.title)	 itemfields["cotrol-title"].itemfield("option", "valid", valid = false);
						if(!control.eId)	 itemfields["cotrol-entityid"].itemfield("option", "valid", valid = false);
						if(!controlUnit.key) itemfields["cotrol-uid"].itemfield("option", "valid", valid = false);
						if(!valid) return;
						
						if(isNew){
							
							Hub.Storage.Control.add(control).then(function ( id ){
								
								if(id <= 0){
									
									UI.report(L("#sea-ui-error-add-code") + id, "", "warning");
									$.sea_ui.questionpanel({
										title     : L("#sea-ui-error-add-code") + L("#sea-ui-error-"+ Hub.errorCodeString(id)),
										appendTo  : "#sea_ui", autoOpen: true, autoRemove: true, decoration: "warning"
									});
									return;
								}
								control.id = id;
								Hub.Storage.ControlSettings.save({pId: id, key: "controlUnit", value: controlUnit}).then(function ( sId ){
									
									if( sId <= 0){
										
										UI.report(L("#sea-ui-error-add-code") + sId, "", "warning");
										$.sea_ui.questionpanel({
											title     : L("#sea-ui-error-add-code") + L("#sea-ui-error-"+ Hub.errorCodeString(sId)),
											appendTo  : "#sea_ui", autoOpen: true, autoRemove: true, decoration: "warning"
										});
										return;
									}
									self.dashboard.addControlUnit(control, controlUnit.key, controlUnit.options);
									that.close();
								}, this._criticalError);
								
							}, this._criticalError);
						} else{
							
							Hub.Storage.Control.update(control).then(function ( id ){
								
								if(id <= 0){
									
									UI.report(L("#sea-ui-error-edit-code") + id, "", "warning");
									$.sea_ui.questionpanel({
										title     : L("#sea-ui-error-edit-code") + L("#sea-ui-error-"+ Hub.errorCodeString(id)),
										appendTo  : "#sea_ui", autoOpen: true, autoRemove: true, decoration: "warning"
									});
									return;
								}
								
								Hub.Storage.ControlSettings.save({pId: id, key: "controlUnit", value: controlUnit}).then(function ( sId ){
									
									if( sId <= 0){
										
										UI.report(L("#sea-ui-error-edit-code") + sId, "", "warning");
										$.sea_ui.questionpanel({
											title     : L("#sea-ui-error-edit-code") + L("#sea-ui-error-"+ Hub.errorCodeString(sId)),
											appendTo  : "#sea_ui", autoOpen: true, autoRemove: true, decoration: "warning"
										});
										return;
									}
									self.dashboard.updateControlUnit(control, controlUnit.key, controlUnit.options);
									that.close();
								}, this._criticalError);
								
							}, this._criticalError);
						}
					}
				},{
					text : L("#remove"),
					class: "remove",
					click: isNew ? null : function ( event, element ){
						
						var that = this;
						$.sea_ui.questionpanel({
							title     : L("#sea-ui-question-item-remove"),
							appendTo  : "#sea_ui",
							autoOpen  : true,
							autoRemove: true,
							buttons   : [{
								text : L("#yes"),
								click: function (){
									
									Hub.Storage.Control.delete(control.id).then(function ( id ){
										
										if(id > 0)
											self.dashboard.deleteControlUnit(control.id);
										else
											UI.report(L("#sea-ui-error-remove-code") + id, "", "warning");
									}, self._criticalError);
									
									that.close();
									this.close();
								}
							},{
								text : L("#no"),
								click: true
							}]
						});
					}
				}]
			})
			.append(itemfields["cotrol-title"] = $("<div>").itemfield({
				type: "input-string",
				name: "cotrol-title",
				label: L("#name"),
				value: control.title
			}))
			.append(itemfields["cotrol-entityid"] = $("<div>").itemfield({
				type: "select-gamecontrol",
				name: "cotrol-entityid",
				label: L("#sea-ui-dialog-label-gamecontrol"),
				value: control.eId,
				gridEntityId: gridEntityId,
				change: function ( value ){
					
				    iControlUnitOptions.property("controlId", Utilities.isGroupId(value) ? Utilities.tryParseJSON(value) : value);
					iControlUnitOptions.build();
				}
			}))
			.append(itemfields["cotrol-uid"] = $("<div>").itemfield({
				type: "select-storagecontrol",
				name: "cotrol-uid",
				label: L("#sea-ui-dialog-label-storagecontrol"),
				value: controlKey,
				change: function ( value ){
					
					iControlUnitOptions.option("key", value);
					iControlUnitOptions.build();
				}
			}))
			.append(iControlUnitOptions.element);
		}
	},
	"control-edit": {
		group: 1,
		options: {
			floatRight: true,
			visible: false
		},
		action: function ( button ){
			
			this.dashboard.option("editable", !button.element.hasClass("check"));
				
			if(this.dashboard.options.editable)
				button.element.addClass("check");
			else
				button.element.removeClass("check");
		}
	},
	"control-drag": {
		group: 1,
		options: {
			floatRight: true,
			visible: false
		},
		action: function ( button ){
			
			this.dashboard.option("draggable", !button.element.hasClass("check"));
				
			if(this.dashboard.options.draggable)
				button.element.addClass("check");
			else
				button.element.removeClass("check");
		}
	},
	"choose": { /* Group 2 */
		group: 2,
		options: {
			floatRight: true,
			visible: true
		},
		action: function ( button ){
			
			this._treeListToggleSelectable(!button.element.hasClass("check"));
				
			if(this.settings.selectable)
				button.element.addClass("check");
			else
				button.element.removeClass("check");
		}
	},
	"add": { /* or Edit*/
		group: 2,
		options: {
			floatRight: true,
			visible: false
		},
		action: function ( button, $item ){
			
			var self = this,
				isNew = !$item,
				item = {
					id   : isNew ? 0 : $item.attr("uid"),
					title: isNew ? "" : $item.attr("title")
				},
				itemfields = {};
			
			
			if( isNew ? this.settings.worldId : ($item.data("name") === "grid") ){/* Grid */
				
				item.pId = this.settings.worldId;
				item.eId = isNew ? "" : $item.attr("eId");
				
				$("<div>")
				.sidebarpanel({
					title     : L(isNew ? "#add" : "#edit") + " " + L("#grid").toLowerCase(),
					appendTo  : "#sea_ui",
					autoOpen  : true,
					autoRemove: true,
					class     : "addOrEditGrid",
					buttons   : [{
						text : L(isNew ? "#add" : "#edit"),
						click: function ( event, element ){
							
							var that = this,
								valid = true;
								
							item.title = itemfields["grid-title"].itemfield("value");
							item.eId   = itemfields["grid-entityid"].itemfield("value");
							
							if(!item.title) itemfields["grid-title"].itemfield("option", "valid", valid = false);
							if(!item.eId)   itemfields["grid-entityid"].itemfield("option", "valid", valid = false);
							if(!valid) return;
							
							if(isNew){
								
								Hub.Storage.Grid.add(item).then(function ( id ){
									
									if(id <= 0){
										
										UI.report(L("#sea-ui-error-add-code") + id, "", "warning");
										$.sea_ui.questionpanel({
											title     : L("#sea-ui-error-add-code") + L("#sea-ui-error-"+ Hub.errorCodeString(id)),
											appendTo  : "#sea_ui", autoOpen: true, autoRemove: true, decoration: "warning"
										});
										return;
									}
									item.id = id;
									self._addGrid(item);
									
									that.close();
								}, this._criticalError);
							}
							else{
								
								Hub.Storage.Grid.update(item).then(function ( id ){
										
									if(id <= 0){
										
										UI.report(L("#sea-ui-error-edit-code") + id, "","warning");
										$.sea_ui.questionpanel({
											title     : L("#sea-ui-error-edit-code") + L("#sea-ui-error-"+ Hub.errorCodeString(id)),
											appendTo  : "#sea_ui", autoOpen: true, autoRemove: true, decoration: "warning"
										});
										return;
									}
									self._updateGrid(item);
									
									that.close();
								}, this._criticalError);
							}
						}
					}]
				})
				.append(itemfields["grid-title"] = $("<div>").itemfield({
					type: "input-string",
					name: "grid-title",
					label: L("#name"),
					value: item.title
				}))
				.append(itemfields["grid-entityid"] = $("<div>").itemfield({
					type: "select-gamegrid",
					name: "grid-entityid",
					label: L("#sea-ui-dialog-label-gameobject"),
					value: item.eId
				}));
			}
			else{/* World */
				
				$("<div>")
				.sidebarpanel({
					title     : L(isNew ? "#add" : "#edit") + " " + L("#world").toLowerCase(),
					appendTo  : "#sea_ui",
					autoOpen  : true,
					autoRemove: true,
					class     : "addOrEditWorld",
					buttons   : [{
						text : L(isNew ? "#add" : "#edit"),
						click: function ( event, element ){
							
							var that = this,
								valid = true;
							
							item.title = itemfields["world-title"].itemfield("value");
							
							if(!item.title) itemfields["world-title"].itemfield("option", "valid", valid = false);
							if(!valid) return;
							
							if(isNew){
								
								Hub.Storage.World.add(item).then(function ( id ){
									
									if(id <= 0){
										
										UI.report(L("#sea-ui-error-add-code") + id, "", "warning");
										$.sea_ui.questionpanel({
											title     : L("#sea-ui-error-add-code") + L("#sea-ui-error-"+ Hub.errorCodeString(id)),
											appendTo  : "#sea_ui", autoOpen: true, autoRemove: true, decoration: "warning"
										});
										return;
									}
									item.id = id;
									self._addWorld(item);
									
									that.close(true);
								}, this._criticalError);
							}
							else{
								
								Hub.Storage.World.update(item).then(function ( id ){
										
									if(id <= 0){
										
										UI.report(L("#sea-ui-error-edit-code") + id, "", "warning");
										$.sea_ui.questionpanel({
											title     : L("#sea-ui-error-edit-code") + L("#sea-ui-error-"+ Hub.errorCodeString(id)),
											appendTo  : "#sea_ui", autoOpen: true, autoRemove: true, decoration: "warning"
										});
										return;
									}
									self._updateWorld(item);
									
									that.close();
								}, this._criticalError);
							}
						}
					}]
				})
				.append(itemfields["world-title"] = $("<div>").itemfield({
					type: "input-string",
					name: "world-title",
					label: L("#name"),
					value: item.title
				}));
			}
		}
	},
	"remove": {
		group: 2,
		options: {
			floatRight: true,
			visible: false
		},
		action: function ( button ){
			
			var items = this._treeListAllTopSelected(), that = this;
			
			if(items.worlds.length > 0 || items.grids.length > 0)
				$.sea_ui.questionpanel({
					title     : L("#sea-ui-question-items-selected-remove"),
					appendTo  : "#sea_ui",
					autoOpen  : true,
					autoRemove: true,
					buttons   : [{
						text : L("#yes"),
						click: function (){
							
							var i = 0, uid;
							/* Worlds remove */
							
							for(; i < items.worlds.length; ++i) (function ( item ){
								
								uid = item.attr("uid");
								if(uid == that.settings.worldId) that._worldId("");
								Hub.Storage.World.delete(uid).then(function ( id ){
											
									if(id > 0){
										
										item.next(".content").remove();
										item.remove();
									}
									else										
										UI.report(L("#sea-ui-error-remove-code") + id, "", "warning");
								}, this._criticalError)
							}(items.worlds.eq(i)));
							/* Grids remove */
							
							for(i = 0; i < items.grids.length; ++i) (function ( item ){
								
								uid = item.attr("uid");
								if(uid == that.settings.gridId) that._gridId("");
								Hub.Storage.Grid.delete(uid).then(function ( id ){
											
									if(id > 0)
										item.remove();
									else
										UI.report(L("#sea-ui-error-remove-code") + id, "", "warning");
								}, this._criticalError)
							}(items.grids.eq(i)));
							
							this.close();
						}
					},{
						text : L("#no"),
						click: true
					}]
				});
		}
	},
	"edit": {
		group: 2,
		options: {
			floatRight: true,
			visible: false
		},
		action: function ( button ){
			
			var item = this._treeListFirstSelected();
			if(item.length > 0)
				this._buttons["add"].action.call(this, this._buttons["add"], item.eq(0));
		}
	},
	"locale": {
		group: 2,
		options: {
			floatRight: false,
			visible: true,
			class: "sea-text"
		},
		constructor: function ( button ){
			
			button.options.select = $("<select class='locale-list' style='display:none;'></select>");
			Languages.getAvailableLocales("sea-ui").then(function ( obj ){
				
				var locale = Languages.locale(),
					key;
				
				if(obj.hasOwnProperty(locale))
					button.element.text(obj[locale].shortname);
				else
					button.element.text("[!]");
				
				for(key in obj)
					$("<option>", {
						"text" : obj[key].localfullname,
						"value": key
					}).appendTo(button.options.select);
			}, function (){
				
				button.element.text("[!]");
			});
		},
		action: function ( button ){
			
			var that = this,
				panel = $.sea_ui.sidebarpanel({
					title	  : L("#language"),
					appendTo  : "#sea_main_menu",
					autoOpen  : true,
					autoRemove: true,
					stretch   : true
				}).element.append($("<div>").optionslist({
					class: "locale-key",
					select: function ( element ){ 
						
						var l = element.attr("value");
						panel.sidebarpanel("close");
						
						if(Languages.locale() === l) return;
						Languages.setDataLocale(element.attr("value"));
						window.location.reload();
					}
				}).optionslist("fill", button.options.select));
		}
	},
	"help": {
		group: 2,
		options: {
			floatRight: false,
			visible: true
		},
		action: function ( button ){
			
			$.sea_ui.sidebarpanel({
				title	  : L("#help"),
				appendTo  : "#sea_ui",
				autoOpen  : true,
				autoRemove: true,
			}).element
			.append(Helpers.paragraph(L("#sea-ui-help-header-about"), L("#sea-ui-help-content-about")))
			.append(Helpers.paragraph(L("#sea-ui-help-header-terms"), L("#sea-ui-help-content-terms")))
			.append(Helpers.paragraph(L("#sea-ui-help-header-how_it_works"), L("#sea-ui-help-content-how_it_works")))
			.append(Helpers.paragraph(L("#sea-ui-help-header-how_to_use"), L("#sea-ui-help-content-how_to_use")))
			.append(Helpers.paragraph(L("#sea-ui-help-header-links"), L("#sea-ui-help-content-links")));
		}
	},
	"fullscreen": {
	    group: 2,
	    options: {
	        floatRight: false,
	        visible: true
	    },
	    action: function (button) {

	        $(document).toggleFullScreen();
	    }
	}},
	
	_create: function (){
		
		this.settings = {
			open: false,
			worldId: "",
			gridId: "",
			gridEntityId: "",
			selectable: false
		};
		var $buttonGroup, group = 0;
		this.buttons = $([]);
		
		for(var name in this._buttons){
			
			var button = this._buttons[name];
			if(group !== button.group){
				
				if(group > 0)
					this.buttons = this.buttons.add($buttonGroup);
				group = button.group;
				$buttonGroup = $("<div class='buttons'></div>");
			}
			
			button.element = $("<span>", {
				class: "button".join20(button.options.floatRight ? "float-r" : "").join20(name).join20(button.options.class),
				style: button.options.visible ? null : "display:none;"
			})
			.data("name", name)
			.append( button.options.special ? "<div class='special'></div>" : null)
			.appendTo($buttonGroup);
			
			if($.type(button.constructor) === "function")
				button.constructor.call(this, button);
		}
		this.buttons = this.buttons.add($buttonGroup);
		
		this.treeList = $("<div class='treelist ui-widget scrollbar-rail noselect'></div>");
		this.element
			.addClass("noselect")
			.append(this.buttons)
			.append(this.treeList)
			.disableSelection();
		
		this.treeList.scrollbar();
		
		this._on(this.buttons,  {"click .button": "_onButtonClick"});
		this._on(this.treeList, {"click .header": "_onTreeListClick"});
		
		this.dashboard = $("#sea_dashboard").dashboard({
			editControlUnit: this._onEditControlUnit = $.proxy(this._onEditControlUnit, this)
		}).dashboard("instance");
		
		/*this._showSessionStatusDialog = $.debounce(2500, this._showSessionStatusDialog);*/
		Hub.Callbacks.onChangeSessionStatus.add(this._changeSessionStatus = $.proxy(this._changeSessionStatus, this));
		Hub.Callbacks.onChangeHubStatus.add(this._changeHubStatus = $.proxy(this._changeHubStatus, this));
	},
	_destroy: function (){
		
		Hub.Callbacks.onChangeSessionStatus.remove(this._changeSessionStatus);
		Hub.Callbacks.onChangeHubStatus.remove(this._changeHubStatus);
	},
	
	_worldId: function ( uId ){
		
		if(uId === undefined)
			return this.settings.worldId;
		
		if(uId !== this.settings.worldId){
			
			this.settings.worldId = uId;
			this.settings.gridId = "";
			this.settings.gridEntityId = "";
			
			this._toggleControlButton(false);
			this.dashboard.load("", "");
		}
	},
	_gridId: function ( uId, eId ){
		
		if(uId === undefined)
			return this.settings.gridId;
		
		if(uId !== this.settings.gridId){
						
			this.settings.gridId = uId;
			this.settings.gridEntityId = !uId ? "" : eId;
			
			this._toggleControlButton(!!uId);
			this.dashboard.load(this.settings.gridId, this.settings.gridEntityId);
		}
	},
	_loadWorlds: function (){
		
		var self = this;
		Hub.Storage.World.getAll().then(function ( arr ){
			
			if ($.type(arr) === "array") {
				
				for(var i = 0; i < arr.length; ++i)
					self._addWorld(arr[i]);
				
				self.treeList.show();
			}
			
			if(self.settings.worldId){
				
				var element = self.treeList.children(".header[uid = " + self.settings.worldId + "]:first");
				if(element.length > 0)
					self._onTreeListClick({currentTarget: element});
				else
					self._worldId("");
			}
		}, function ( err ){
			
			self._worldId("");
			UI.report(L("#errorGetStorageData"), err, "error");
		});
	},
	_loadGrids: function ( content, worldId ){
		
		var self = this;
		Hub.Storage.Grid.getAll(worldId).then(function ( arr ){
			
			if ($.type(arr) === "array")
				for(var i = 0; i < arr.length; ++i){
					
					if(worldId !== self.settings.worldId)
						return;
					
					self._addGrid(arr[i], content);
				}
			
			if(self.settings.gridId){
				
				var element = content.children(".header[uid = " + self.settings.gridId + "]:first");
				if(element.length > 0)
					self._onTreeListClick({currentTarget: element});
				else
					self._gridId("");
			}
		}, function ( err ){
			
			self._gridId("");
			UI.report(L("#errorGetStorageData"), err, "error");
		});
		
		content.show();
	},
	_addWorld: function ( world ){
		
		this.treeList.append($("<div>", {
			"text" : world.title,
			"title": world.title,
			"class": this.settings.selectable ? "header checkbox" : "header",
			"uid"  : world.id
		}).data("name", "world"))
		.append("<div class='content' style='display:none;'></div>");
	},
	_addGrid: function ( grid, element ){
		
		if(!element){
			
			element = this.treeList.children(".header[uid = " + grid.pId + "]:first");
			if(element.length === 0) return;
			element = element.next(".content");
		}
		
		element.append($("<div>",{
			"text" : grid.title,
			"title": grid.title,
			"class": this.settings.selectable ? "header checkbox" : "header",
			"uid"  : grid.id,
			"eid"  : grid.eId
		}).data("name", "grid"));
	},
	_updateWorld: function ( world ){
		
		switch($.type(world)){
			
			case "string":
			case "number":
				
				var self = this;
				Hub.Storage.World.get(world).then(function ( world ){
					
					if ($.type(world) !== "object")
						return;
						
					var element = self.treeList.children(".header[uid = " + world.id + "]:first");
					if(element.length > 0)
						element
							.text(world.title)
							.attr("title", world.title);
				});
			return;
			case "object":
				
				var element = this.treeList.children(".header[uid = " + world.id + "]:first");
				if(element.length > 0)
					element
						.text(world.title)
						.attr("title", world.title);
			return;
		}
	},
	_updateGrid: function ( grid ){
		
		switch($.type(grid)){
			
			case "string":
			case "number":
				
				var self = this;
				Hub.Storage.Grid.get(grid).then(function ( grid ){
					
					if ($.type(grid) !== "object")
						return;
						
					var element = self.treeList.find(".content>.header[uid = " + grid.id + "]:first");
					if(element.length > 0)
						element
							.text(grid.title)
							.attr("title", grid.title)
							.attr("eid", grid.eId);
				});
			return;
			case "object":
				
				var element = this.treeList.find(".content>.header[uid = " + grid.id + "]:first");
				if(element.length > 0)
					element
						.text(grid.title)
						.attr("title", grid.title)
						.attr("eid", grid.eId);
			return;
		}
	},
	_criticalError: function ( err ){
		
		UI.report(L("#sea-ui-error-critical"), err, "error");
		$.sea_ui.questionpanel({
			title     : L("#sea-ui-error-critical") + err,
			appendTo  : "#sea_ui", autoOpen: true, autoRemove: true, decoration: "error",
			closeDelay: 5000
		});
	},
	
	_toggleControlButton: function ( visible ){
        
        if(visible)
            this.buttons.find(".control-add, .control-edit, .control-drag").show();
        else
            this.buttons.find(".control-add, .control-edit, .control-drag").hide();
    },
	_treeListToggleSelectable: function ( selectable ){
		
		this.settings.selectable = selectable;
		if(selectable){
			
			this.treeList.find(".header").addClass("checkbox");
			this.buttons.find(".edit, .remove").show();
			this.buttons.find(".add").hide();
		}
		else{
			
			this.buttons.find(".add").show();
			this.buttons.find(".edit, .remove").hide();
			this.treeList.find(".header").removeClass("checkbox check");
		}
	},
	_treeListFirstSelected: function (){
		return this.treeList.find(".header.checkbox.check:first");
	},
	_treeListAllTopSelected: function (){
		
		var result = {
				worlds: $([]),
				grids: $([])
			},
			grids = this.treeList.find(".content>.header.checkbox.check"),
			parent_uid = 0;
			
		if(grids.length > 0){
			
			var parent = grids.first().parent().prev();
			if(parent.hasClass("check")){
				
				parent_uid = parent.attr("uid");
				result.worlds = result.worlds.add(parent);
			}
			else
				result.grids = grids;
		}
		
		result.worlds = result.worlds.add(this.treeList.children(".header.checkbox.check" + ( parent_uid > 0 ? "[uid != '"+ parent_uid +"']" : "" )));
		
		return result;
	},
	
	_onEditControlUnit: function ( event, element ){
		
		this._buttons["control-add"].action.call(this, this._buttons["control-add"].element, element);
	},
	_onButtonClick: function ( event ){
		
		var button = this._buttons[$(event.currentTarget).data("name")];
		button.action.call(this, button);
		
		return false;
	},
	_onTreeListClick: function ( event ){
		
		var $header = $(event.currentTarget),
			$content = $header.next(".content"),
			itemName = $header.data("name");
		
		if(this.settings.selectable){
			
			$header.toggleClass("check");
			if($header.hasClass("check"))
				$content.find(".header").addClass("check");
			else
				$content.find(".header").removeClass("check");
			
			if(itemName === "grid")
				$header.parent().prev().removeClass("check");
		} else{
			
			$content.siblings(".content:visible").empty().hide();
			$header.siblings(".header.selected").removeClass("selected");
			$content.siblings(".content:selected").removeClass("selected");
			
			if($content.is(":visible")){
				
				$header.removeClass("selected");
				$content.removeClass("selected").empty().hide();
				
				switch (itemName){
					case "world": this._worldId(""); break;
					case "grid" : this._gridId(""); break;
				}
			} else {
				
				$header.addClass("selected");
				$content.addClass("selected");
				
				switch (itemName){
					case "world":
						this._loadGrids($content, $header.attr("uid"));
						this._worldId($header.attr("uid"));
					break;
					case "grid" :
						this._gridId($header.attr("uid"), $header.attr("eid"));
					break;
				}
			}
		}
		
		return false;
	},
	_changeSessionStatus: function ( status ){
		
		switch (status){
			
			case "online" : this.buttons.children(".status").removeClass("game-pending").addClass("game-online"); break;
		    case "standby": this.buttons.children(".status").removeClass("game-online").addClass("game-pending"); break;
		    default: this.buttons.children(".status").removeClass("game-online game-pending"); break;
		}
		/*this._showSessionStatusDialog(status);*/
	},
	/*_showSessionStatusDialog: function ( status ){

	    if (status === "online" || status === "offline" || status === "standby") return;

	    $.sea_ui.questionpanel({
	        title: L("#sea-ui-dialog-label-sessionstatus") + ": " + L("#sea-ui-error-" + status),
	        appendTo: "#sea_ui", autoOpen: true, autoRemove: true, decoration: "warning"
	    });
	},*/
	_changeHubStatus: function ( status ){
		
		switch (status){
			
			case "online" : this.buttons.children(".status").removeClass("hub-pending").addClass("hub-online"); break;
			case "standby": this.buttons.children(".status").removeClass("hub-online").addClass("hub-pending"); break;
			default       : this.buttons.children(".status").removeClass("hub-online hub-pending"); break;
		}
	}
});
$.widget( "sea_ui.sidebarpanel", {
	version: "1.0.0",
	options: {
		title     : null,
		appendTo  : "body",
		buttons   : [],
		stretch	  : false,
		autoOpen  : false,
		autoRemove: false,
		class     : "",
		//Events
		close: $.noop
	},
	
	_create: function (){
		
		/*this.originalParent = this.element.parent();*/
		var btnClose = $("<span class='btn-close'></span>");
		this.header = $("<span class='header sea-text'></span>").text(this.options.title).append(btnClose);
		
		if(this.options.buttons.length > 0){
			
			this.buttons = $("<span class='buttons'></span>");
			for(var i = 0; i < this.options.buttons.length; ++i){
				
				var btn = this.options.buttons[i];
				if($.type(btn.click) !== "function") continue;
				
				$("<span>", {
					text : btn.text,
					class: "button sea-text".join20(btn.class)
				})
				.data("uid", i)
				.appendTo(this.buttons);
			}
		}
		else
			this.buttons = null;
		
		this.dialog =
			$("<div>", {
				class: "sea-sidebar-panel noselect".join20(this.options.class),
				style: this.options.autoOpen ? "display:block;" : "display:none;"
			})
			.append(this.header)
			.append(this.buttons)
			.append(this.element.addClass("content scrollbar-rail".join20(this.buttons ? "" : "nobutton")))
			.appendTo(this._appendTo());
		
		if(this.options.stretch)
			this.dialog.addClass("stretch");
		else{
			
			this.dialog.resizable({
				containment: "parent",
				handles: "e",
				minWidth: 245
			}).children(".ui-resizable-e").attr("class", "sea-ui-resizable-e");
			this._on(window, {"throttledresize": "_onResize"});
		}
		this.element.scrollbar();
		if(this.buttons)
			this._on(this.buttons, {"click .button": "_onButtonClick"});
		this._on(btnClose, {"click": "close"});
	},
	_destroy: function (){
		
		this._off(window, "throttledresize");
	},
	
	_appendTo: function (){
		
		var element = this.options.appendTo;
		if(element && (element.jquery || element.nodeType))
			return $(element);
		
		return this.document.find(element || "body").eq(0);
	},
	
	open: function (){
		
		this.dialog.show();
	},
	close: function (){
		
		if(this.options.autoRemove)
			this.dialog.remove();
		else
			this.dialog.hide();
		
		this.options.close();
	},
	
	_onResize: function ( event ){
		
		var w_width = $(window).width();
		if(this.dialog.width() > w_width)
			this.dialog.width(w_width);
	},
	_onButtonClick: function ( event ){
		
		var element = $(event.currentTarget);
		this.options.buttons[element.data("uid")].click.call(this, event, element);
		
		return false;
	}
});
$.widget( "sea_ui.questionpanel", {
	version: "1.0.0",
	options: {
		title     : null,
		appendTo  : "body",
		buttons   : [],
		autoOpen  : false,
		autoRemove: false,
		class     : "",
		decoration: "",
		closeDelay: 0,
		//Events
		close: $.noop
	},
	_availableDecoration: ["", "warning", "error"],
	_closeDelay: {
		min: 3000,/* 3sec */
		max: 30000,/* 30sec */
		normalize: function ( delay ){ return !delay ? 0 : (delay > this.max ? this.max : (delay < this.min ? this.min : delay)); }
	},
	
	_create: function (){
		
		this.header = $("<div class='header sea-text'></div>").text(this.options.title);
		
		if(this.options.buttons.length === 0){
			this.options.buttons.push({
				text: L("#ok"),
				click: function (){ this.close(); }
			});
		}
		this.buttons = $("<div class='buttons'></div>");
		var btn, i = 0;
		for(; i < this.options.buttons.length; ++i){
			
			btn = this.options.buttons[i];
			switch($.type(btn.click)){
				case "function": break;
				case "boolean" : btn.click = function (){this.close();}; break;
				default: continue; break;
			}
			
			$("<span>", {
				text : btn.text,
				class: "button sea-text "+ (btn.class || "")
			})
			.data("uid", i)
			.appendTo(this.buttons);
		}
		if($.inArray(this.options.decoration, this._availableDecoration) === -1)
			this.options.decoration = "";
			
		var overlay = $("<div class='overlay'></div>");
		this.dialog =
			$("<div>", {
				class: "sea-questionpanel noselect".join20(this.options.class),
				style: this.options.autoOpen ? "display:block;" : "display:none;"
			})
			.append(overlay)
			.append(this.element
				.addClass("line".join20(this.buttons ? null : "nobutton").join20(this.options.decoration))
				.append(this.header)
				.append(this.buttons))
			.appendTo(this._appendTo());
		
		this._on(this.buttons, {"click .button": "_onButtonClick"});
		this._on(overlay, {"click": "close"});
		
		this.options.closeDelay = this._closeDelay.normalize(this.options.closeDelay);
		if(this.options.closeDelay > 0){
			
			var then = this;
			setTimeout(function (){ then.close(); }, this.options.closeDelay);
		}
	},
	
	_appendTo: function (){
		
		var element = this.options.appendTo;
		if(element && (element.jquery || element.nodeType))
			return $(element);
		
		return this.document.find(element || "body").eq(0);
	},
	
	open: function (){
		
		this.dialog.show();
	},
	close: function (){
		
		if(this.options.autoRemove)
			this.dialog.remove();
		else
			this.dialog.hide();
		
		this.options.close();
	},
	
	_onButtonClick: function ( event ){
		
		var element = $(event.currentTarget);
		this.options.buttons[element.data("uid")].click.call(this, event, element);
		
		return false;
	}
});
$.widget( "sea_ui.optionslist", {
	version: "1.0.0",
	options: {
		class : "",
		//Events
		select: $.noop
	},
	
	_create: function (){
		
		this.element.addClass("sea-ui-optionslist");
		this._on(this.element, {"click .item": "_onSelect"});
	},
	clear: function (){
		this.element.empty();
	},
	fill: function ( select, attrsToClass, element ){
		
		if(!select) return;
		
		if(element === undefined){
			
			this.element.empty();
			element = this.element;
		}
		
		var $options = select.children("option, optgroup"),
			i = 0, j,
			originalOption,
			option,
			hasAttrs = $.type(attrsToClass) === "array" && attrsToClass.length > 0;
		
		for(; i < $options.length; ++i){
			
			originalOption = $options.eq(i);
			if(originalOption.is("optgroup")){
				
				option =
				$("<div class='item-group-header sea-text".join20(this.options.class) +"'></div>")
				.text(originalOption.attr("label"))
				.appendTo(element);
				
				option = $("<div class='item-group-content'></div>");
				this.fill(originalOption, attrsToClass, option);
			} else{
				
				option =
				$("<div class='item sea-text".join20(this.options.class) +"'></div>")
				.attr("value", originalOption.attr("value"))
				.text(originalOption.text());
				
				if(hasAttrs){
					
					j = attrsToClass.length;
					while(--j >= 0)
						option.addClass(originalOption.attr(attrsToClass[j]));
				}
			}
			option.appendTo(element);
		}
	},
	_onSelect: function ( event ){
		
		this.options.select($(event.currentTarget));
	}
});
$.widget( "sea_ui.itemfield", {
	version: "1.0.0",
	defaultElement: "<div>",
	options: {
		orientation: "vertical",/*[vertical, horizontal]*/
		type: "",
		name: "",
		label: null,
		value: null,
		valid: true,
		//Events
		change: $.noop
	},
	_elements: {},
	
	_create: function (){
		
		this.element.addClass("sea-ui-itemfield".join20(this.options.orientation).join20(this.options.name));
		if(this.options.label)
			$("<span>", {
				text : this.options.label,
				class: "label sea-text",
				for  : this.options.name
			}).appendTo(this.element);
		
		this.hideValidationIcon = $.debounce(3000, function (){
			
			this.element.removeClass("invalid-value");
		});
		this._setValid(this.options.valid);
		
		var element = this._elements[this.options.type];
		element.constructor.call(this, element);
		if(element.click)
			this._on(this.wrapper, {"click": "_onClick"});
	},
	_setOption: function ( key, value ){
		
		switch(key){
			case "value":
				
				this.value(value);
				return false;
			case "valid":
				
				this._setValid(value);
			break;
		}
		
		this._super(key, value);
	},
	_setValid: function ( val ){
		
		if(val)
			this.element.removeClass("invalid-value");
		else{
			
			this.element.addClass("invalid-value");
			this.hideValidationIcon();
		}
	},
	
	_onClick: function ( event ){
		
		return this._elements[this.options.type].click.call(this, event);
	},
	value: function (){
		
		var f = this._elements[this.options.type].value;
		return f ? f.apply(this, arguments) : undefined;
	}
});

/* Extension sea_ui.itemfield */
(function ( proto ){

$.extend(proto, {
"_sea_input": {
	constructor: function ( input_type ){
		
		this.field = $("<input>", {
			value: this.options.value,
			class: "value sea-text".join20(this.options.name) +" type-"+ this.options.type,
			type : input_type || "text",
			name : this.options.name
		});
		
		this.wrapper = $("<span class='input-wrapper'></span>")
		.append(this.field)
		.appendTo(this.element);
		
		var f = $.debounce(200, function ( event ){
			
			this.value(this.field.val(), true);
		});
		this._on(this.field, {"keyup": f, "paste": f} );
	},
	value: function ( val, regenerate ){
		
		if(val === undefined)
			return this.options.value;/*this.field.val();*/
		else{
			
			if(regenerate !== true)
				this.field.val(val);
			this.options.value = val;
			this.options.change(this.options.value);
		}
	}
},
"_sea_select": {
	constructor: function ( title, appendTo ){
		
		this.field = $("<select>", {
			class: "value sea-text".join20(this.options.name) +" type-"+ this.options.type,
			style: "display:none;",
			type : "text",
			name : this.options.name
		});
		this.fieldView = $("<span>", {
			class: "v-value sea-text"
		});
		
		this.wrapper = $("<span class='select-wrapper'></span>")
		.append(this.field)
		.append(this.fieldView)
		.appendTo(this.element);
		
		this.fieldList = $.sea_ui.sidebarpanel({
			title   : title,
			appendTo: appendTo,
			autoOpen: false,
			stretch : true,
			close   : $.proxy(function (){
				this.expand = false;
			}, this)
		});
	},
	value: function ( val ){
		
		if(val === undefined)
			return this.options.value;/*this.field.val();*/
		else{
			
			this.field.val(val);
			this.options.value = this.field.val();
			this.fieldView.text(this.field.find(":selected").text());
			this.options.change(this.options.value);
		}
	},
	click: function ( event ){
		
		this.expand = !this.expand;
		if(this.expand)
			this.fieldList.open();
		else
			this.fieldList.close();
		
		return false;
	},
	fillOptionList: function ( _select, _class ){
		
		var that = this;
		that.fieldList.element.append($("<div>").optionslist({
			class: _class,
			select: function (){ _select.apply(that, arguments); }
		}).optionslist("fill", that.field, ["type"]));
	}
}});
$.extend(proto._elements, { 
"input-string": {
	constructor: function (){
		this._sea_input.constructor.call(this);
	},
	value: proto._sea_input.value,
},
"select-gamegrid": {
	optionSelect: function ( element ){
		
		this.value(element.attr("value"));
		this.fieldList.close();
	},
	constructor: function ( element ){
			
		this._sea_select.constructor.call(this, L("#grids"), ".sea-sidebar-panel.addOrEditGrid");
		var self = this;
		
		Hub.Game.Grid.getAll().then(function ( arr ){
			
			if(!arr) return;
			
			for(var i = 0; i < arr.length; ++i)
				$("<option>", {
					"text" : arr[i].text,
					"value": arr[i].eId,
					"type" : arr[i].type/* [station, large, small] */
				}).appendTo(self.field);
			
			if(self.options.value)
				self.value(self.options.value);
			
			self._sea_select.fillOptionList.call(self, element.optionSelect, "grid");
		});
	},
	value: proto._sea_select.value,
	click: proto._sea_select.click
},
"select-gamecontrol": {
	optionSelect: function ( element ){
		
		this.value(element.attr("value"));
		this.fieldList.close();
	},
	constructor: function ( element ){
			
		this._sea_select.constructor.call(this, L("#sea-ui-dialog-label-gamecontrols"), ".sea-sidebar-panel.addOrEditControl");
		var self = this;
		
		if(!self.options.gridEntityId) return;
		Hub.Game.Control.getAll(self.options.gridEntityId).then(function ( arr ){
			
			if(!arr) return;
			for(var i = 0; i < arr.length; ++i)
				$("<option>", {
					"text" : arr[i].text,
					"value": arr[i].type === "group" ? '{"id":"' + self.options.gridEntityId + '","name":"' + arr[i].eId + '","aggr":false}' : arr[i].eId,
					"type" : arr[i].type/* [block, group] */
				}).appendTo(self.field);
			
			if(self.options.value)
			    self.value(self.options.value);
			
			self._sea_select.fillOptionList.call(self, element.optionSelect, "control");
		});
	},
	value: proto._sea_select.value,
	click: proto._sea_select.click
},
"select-storagecontrol": {
	optionSelect: function ( element ){
		
		this.value(element.attr("value"));
		this.fieldList.close();
	},
	constructor: function ( element ){
			
		this._sea_select.constructor.call(this, L("#sea-ui-dialog-label-storagecontrols"), ".sea-sidebar-panel.addOrEditControl");
		
		if(!("controlunit" in $)) return;
		
		var self = this, 
			langs = [],
			lang;
		for(var key in $.controlunit){
			
			lang = $.type($.controlunit[key].prototype.lang) === "string" ? $.controlunit[key].prototype.lang : null;
			if(lang && !Languages.exist(lang))
				langs.push(Languages.load(lang))
		}
		$.when.apply($, langs).then(function (){
			
			var arr = [], __controlUnit, i;
			for(i in $.controlunit){
				
				__controlUnit = $.controlunit[i].prototype;
				arr.push({
					value: i,
					group: L(__controlUnit.group || "#default-controls"),
					text : L(__controlUnit.text),
					order: __controlUnit.order || 0
				});
			}
			/*Sort by Group & Order & Text*/
			arr.sort(function ( a, b ){
				
				return	(a.group < b.group) ? -1 : 
							((a.group > b.group) ? 1 :
								((a.order < b.order) ? -1 :
									((a.order > b.order) ? 1 :
										((a.text < b.text) ? -1 :
											((a.text > b.text) ? 1 :
												0)))));
			});
			/* - - - - */
			
			var group = null, $optgroup;
			for(i = 0; i < arr.length; ++i){
				
				if(group !== arr[i].group){
					
					group = arr[i].group;
					$optgroup = $("<optgroup>", {
						"label": arr[i].group
					}).appendTo(self.field);
				}
				
				$("<option>", {
					"text" : arr[i].text,
					"value": arr[i].value
				}).appendTo($optgroup);
			}
			if(self.options.value)
				self.value(self.options.value);
			
			self._sea_select.fillOptionList.call(self, element.optionSelect, "controlunit");
		});
	},
	value: proto._sea_select.value,
	click: proto._sea_select.click
},
"control-option": {
	optionSelect: function ( element ){
		
		this.value(element.attr("value"));
		this.fieldList.close();
	},
	constructor: function ( element ){
		
		this.hasList = !!this.options.list;
		if(this.hasList){
			
			var that = this,
				value = that.options.value;
			that._sea_select.constructor.call(that, that.options.label, ".sea-sidebar-panel.addOrEditControl");
			that.value("");
			switch($.type(that.options.list)){
				case "array":
					
					if(that.options.list.length > 0){
						
						for(var i = 0; i < that.options.list.length; ++i)
							$("<option>", that.options.list[i]).appendTo(that.field);
						
						if(value !== undefined)
							that.value(value);
					}
					
					that._sea_select.fillOptionList.call(that, element.optionSelect, "control-option");
				break;
				case "function":
					
					that.options.list.call(that.options.properties, function ( arr ){
						
						if($.type(arr) === "array" && arr.length > 0){
							
							for(var i = 0; i < arr.length; ++i)
								$("<option>", arr[i]).appendTo(that.field);
							
							that._sea_select.fillOptionList.call(that, element.optionSelect, "control-option");
						}
						
						if(value !== undefined)
							that.value(value);
					});
				break;
			}
		} else
			this._sea_input.constructor.call(this);
	},
	value: function ( val ){
		
		return this[this.hasList ? "_sea_select" : "_sea_input"].value.call(this, val);
	},
	click: function ( event ){
		
		if(this.hasList)
			return this._sea_select.click.call(this);
	}
}
});
}($.sea_ui.itemfield.prototype))

$.widget( "sea_ui.controlunitoptions", {
    version: "1.0.0",
    options: {
		key: "",
		unitOptions: null,
		name: ""
    },
    _create: function (){
        
		this.properties = {};
        this.element
		.hide()
		.addClass("sea-ui-controlunitoptions".join20(this.options.name))
		.append($("<div class='header sea-text'>"+ L("#settings") +":</div>"));
    },
    _fields: [],
	
    _unitView: function (){
        
        if(!("controlunit" in $) || !$.controlunit.hasOwnProperty(this.options.key)) return null;
            
		var __controlUnit = $.controlunit[this.options.key].prototype,
			options = {};
		
		if($.isPlainObject(__controlUnit.optionsView) && !$.isEmptyObject(__controlUnit.optionsView) &&
		   $.isPlainObject(__controlUnit.options) && !$.isEmptyObject(__controlUnit.options)){
			
			for(var key in __controlUnit.optionsView){
				
				var optionView = __controlUnit.optionsView[key],
					unitOption = __controlUnit.options[key],
					option = { name: L(optionView.text) },
					sType = this.options.unitOptions ? $.type(this.options.unitOptions[key]) : "null",
					uType = $.type(unitOption);
				
				switch(uType){
					case "string":
					case "boolean":
						option.value = sType === uType ? this.options.unitOptions[key] : unitOption;
					break;
					case "number":
						option.min = optionView.min;
						option.max = optionView.max;
						option.value = Utilities.normalizeNumeric(sType === uType ? this.options.unitOptions[key] : unitOption, option.min, option.max);
					break;
				}
				
				if("value" in option){
					
					option.type = uType;
					if(optionView.list)
						switch($.type(optionView.list)){
							case "array":
								
								option.list = jQuery.extend(true, [], optionView.list);
								for(var lKey in option.list)
									option.list[lKey].text = L(option.list[lKey].text);
							break;
							case "function":
								
								option.list = optionView.list;
							break;
						}
					else if(option.type === "boolean")
						option.list = [ {text: L("#yes"), value: true}, {text: L("#no"), value: false} ];
					
					options[key] = option;
				}
			}
		}
		else
			return null;
		
		return {
			group  : L("group" in __controlUnit ? "" + __controlUnit.group : "#default-controls"),
			name   : L(__controlUnit.text),
			options: options
		}
    },
    build: function (){
        
		this._fields = [];
		this.element.children(":not(.header)").remove();
		
        if(!this.options.key) return;
        
		var unitView = this._unitView(),
			key, optionView, optionField;
        
		if(!unitView || $.isEmptyObject(unitView.options)) return;
        for(key in unitView.options){
            
			optionView = unitView.options[key];
			optionField = $.sea_ui.itemfield({
				orientation	: "horizontal",
				type		: "control-option",
				name		: key,
				label		: optionView.name,
				value		: optionView.value.toString(),
				valueType	: optionView.type,
				list		: "list" in optionView ? optionView.list : null,
				properties	: "list" in optionView ? this.properties : null,
			});
			
			optionField.element
				.data("uid", this._fields.push(optionField) -1)
				.appendTo(this.element);
        }
		
		if(this._fields.length > 0)
			this.element.show();
    },
	_parseToType: function ( val, type ){
		
		switch(type){
			case "string" : return ""+ val;
			case "boolean": return val === "true";
			case "number" : return parseFloat(val);
			default       : return null;
		}
	},
    unitOptions: function (){
        
		var options = {},
			i = 0,
			field;
		for (; i < this._fields.length; ++i){
			
			field = this._fields[i];
			options[field.options.name] = this._parseToType(field.value() || "", field.options.valueType);
		}
		
		return options;
    },
	property: function ( key, value ){
		
		if(!key) return;
		if(value === undefined)
			return this.properties[key];
		
		this.properties[key] = value;
	}
});

var Helpers = (function (){
	
	return{
		paragraph: function (){
			
			if(arguments.length < 2)
				return "";
			if(arguments.length === 2)
				return "<div class='sea-text-normal sea-ui-paragraph'><h>"+ arguments[0] +"</h><div>"+ arguments[1] +"</div></div>";
			
			var text = "<div class='sea-text-normal sea-ui-paragraph'><h>"+ arguments[0] +"</h>", i = 1;
			
			for(; i < arguments.length; ++i)
				text += "<div>"+ arguments[i] +"</div>";
			text += "</div>";
			return text;
		}
	}
}());

