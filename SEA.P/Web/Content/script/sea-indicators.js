﻿
/*
 using: sea-controls.js
 using: sea-controls*.json
 */

/**
  * Default indicators "S.E.A. Multi-touch"
  * Copyright 2016, "OniVe" Igor Kuritsin
  **/

$.widget("controlunit.sea_indicator_ordinal_scale", $.sea_ui.controlunit, {
    version: "1.0.0",
    options: {
        valueLabel  : 0,
        width       : 35,
        height      : 70,
        property    : "",
        value       : 0,
        roundValue  : false,
        metricPrefix: 999,
        unitPrefix  : "",
        minValue    : 0,
        maxValue    : 100
    },
    _minLength: 70,

    lang : "sea-indicators",
    css  : "sea-indicators",
    group: "#sea-indicators-multitouch",
    text : "#sea-indicator-ordinal_scale",
    order: 2,
    optionsView: {
        width       : { text: "#width", min: 35, max: 350 },
        height      : { text: "#height", min: 35, max: 350 },
        minValue    : { text: "#min" },
        maxValue    : { text: "#max" },
        defaultValue: { text: "#defaultValue" },
        roundValue  : { text: "#sea-control-slider-round" },
        metricPrefix: {
            text: "#sea-control-slider-metricPrefix", list: [
                { text: "#sea-control-slider-metricPrefix-disable"   , value: 999 },
                { text: "#sea-control-slider-metricPrefix-base_1E-12", value: -4 },
                { text: "#sea-control-slider-metricPrefix-base_1E-9" , value: -3 },
                { text: "#sea-control-slider-metricPrefix-base_1E-6" , value: -2 },
                { text: "#sea-control-slider-metricPrefix-base_1E-3" , value: -1 },
                { text: "#sea-control-slider-metricPrefix-base_1E0"  , value: 0 },
                { text: "#sea-control-slider-metricPrefix-base_1E3"  , value: 1 },
                { text: "#sea-control-slider-metricPrefix-base_1E6"  , value: 2 },
                { text: "#sea-control-slider-metricPrefix-base_1E9"  , value: 3 },
                { text: "#sea-control-slider-metricPrefix-base_1E12" , value: 4 }
            ]
        },
        unitPrefix  : { text: "#sea-indicators-unitPrefix" },
        property    : { text: "#property", list: function (response) { if (this.controlId) Hub.Game.getBlockPropertiesFloat(this.controlId).always(response); } }
    },

    _afterCreate: function () {

        if (this._eIdIsGroup()) this.options.eId.aggr = true;

        var vertical = this.options.height >= this.options.width;
        if (this.options[vertical ? "height" : "width"] < this._minLength)
            this.options[vertical ? "height" : "width"] = this._minLength;
        this.options.value = Utilities.normalizeNumeric(this.options.value, this.options.minValue, this.options.maxValue);



        this.track = $("<div class='track'></div>");
        this.slider = $("<div class='slider' style='" + (vertical ? "height:18" : "width:18") + "px;'></div>");

        this.element
			.width(this.options.width)
			.height(this.options.height)
			.append($("<div class='sea-controlunit sea-indicator ordinal_scale'></div>")
				.append(this.track
					.append(this.slider)));

        this.settings = {
            vertical       : vertical,
            maxPos         : vertical ? this.track.innerHeight() - this.slider.outerHeight() : this.track.innerWidth() - this.slider.outerWidth(),
            pos            : 0,
            posKey         : vertical ? "top" : "left",
            myPosPrefix    : vertical ? "left top+" : "left+",
            myPosSuffix    : vertical ? "" : " top",
            useMetricPrefix: this.options.metricPrefix < 999,
            unitPrefix     : L(this.options.unitPrefix)
        };

        this.usingSliderValueToPosition = $.proxy(function (pos) {

            this.slider.css(this.settings.posKey, pos[this.settings.posKey]);
        }, this);
        this._externalValueUpdate = $.proxy(this._externalValueUpdate, this);

        this._externalValueUpdate(this.options.value);
        this._syncValue();
    },
    _destroy: function () {

        Hub.Game.removeValueTracking(this.options.eId, this.options.property, this._externalValueUpdate);
    },

    _syncValue: function () {

        Hub.Game.getValueFloat(this.options.eId, this.options.property).then(this._externalValueUpdate);
        Hub.Game.addValueTracking(this.options.eId, this.options.property, this._externalValueUpdate);
    },
    _externalValueUpdate: function (value) {

        if (value === undefined || value === null)
            return;

        if (!this.settings.useMetricPrefix)
            value = this.options.roundValue ? Math.round(value) : Math.round(value * 100) / 100;

        if (value > this.options.maxValue)
            value = this.options.maxValue;
        else if (value < this.options.minValue)
            value = this.options.minValue;

        this.options.value = value;
        this._valueToPosition();
        this._setValueLabel();
    },
    _valueToPosition: function () {

        var pos, oldPos = this.settings.pos;
        if (this.options.value <= this.options.minValue)
            pos = this.settings.vertical ? this.settings.maxPos : 0;
        else if (this.options.value >= this.options.maxValue)
            pos = this.settings.vertical ? 0 : this.settings.maxPos;
        else {

            pos = Math.round(this.settings.vertical ?
                this.settings.maxPos * (this.options.value - this.options.maxValue) / (this.options.minValue - this.options.maxValue) :
                this.settings.maxPos * (this.options.value - this.options.minValue) / (this.options.maxValue - this.options.minValue));

            if (pos > this.settings.maxPos)
                pos = this.settings.maxPos;
            else if (pos < 0)
                pos = 0;
        }

        this.settings.pos = pos;
        if (pos !== oldPos) {

            this.slider.position({
                within   : this.track,
                of       : this.track,
                at       : "left top",
                my       : this.settings.myPosPrefix + this.settings.pos + this.settings.myPosSuffix,
                collision: "fit fit",
                using    : this.usingSliderValueToPosition
            });
            return true;
        }
        else
            return false;
    },
    _setValueLabel: function () {

        this.setValueLabel(this.settings.useMetricPrefix ?
            Utilities.formattedValueInMetricPrefix(this.options.value, this.options.metricPrefix, this.settings.unitPrefix, this.options.roundValue) :
            this.options.value, true);
    }
});