
var Languages = (function(){
    
    var locale = null,
		detail = {},
		defaultLocale = "en",
		locales = $.Deferred(),
		defaultLocales = {"en": {fullname: "English", localfullname: "English", shortname: "EN"}},
		dictionary = {},
		files = { "__null__": $.Deferred().resolve() };
    
	function _loadFile ( name ){
		
		try{
			jQuery.getJSON("/res/lang_" + name + "_" + locale + "_dictionary.json")
			.then(function ( json ){
				
				jQuery.extend(dictionary, json);
				files[name].resolve();
			}, function (){
				
				try{
					/* Try load default dictionary */
					jQuery.getJSON("/res/lang_" + name + "_" + defaultLocale + "_dictionary.json")
					.then(function ( json ){
				
						jQuery.extend(dictionary, json);
						files[name].resolve();
					}, files[name].reject);
				}
				catch(err){
					files[name].reject();
				}
			});
		}
		catch(err){
			files[name].reject();
		}
	}
	
	return{
		localeDetailed: function (){
			return detail;
		},
		locale: function ( l ){
			
			if(l === undefined) return locale;
			
			locale = l;
			for(var name in files)
				if(files[name].state() === "pending")
					_loadFile(name);
		},
		getDataLocale: function (){
			return Hub.Storage.Data.load('locale').promise();
		},
		setDataLocale: function ( locale ){
			return Hub.Storage.Data.save("locale", locale).promise();
		},
		getAvailableLocales: function ( name ){
			
		    if (locales.state() === "pending")
		        $.ajax({ url: "/res/?command=createifnotexists" })
                .always(function () {

                    $.ajax({ url: "/res/?pattern=lang_" + name + "_*_info.json" })
				    .then(function (data) {

				        if ($.type(data) !== "array" || data.length === 0) {

				            locales.resolve(defaultLocales);
				            return;
				        }
				        var i = data.length, _locales = {}, _files = [];
				        try {
				            while (i-- > 0)
				                _files.push(jQuery.getJSON("/res/" + data[i]));

				            $.when.apply($, _files).then(function () {

				                /* Order by "fullname" */
				                var args = Array.prototype.slice.call(arguments).sort(function (a, b) { return (a[0].fullname > b[0].fullname ? -1 : (a[0].fullname < b[0].fullname ? 1 : 0)); });
				                i = args.length;
				                while (i-- > 0)
				                    _locales[args[i][0].locale] = args[i][0];

				                locales.resolve(_locales);
				            }, function () {
				                locales.resolve(defaultLocales);
				            });
				        }
				        catch (err) {
				            locales.resolve(defaultLocales);
				        }
				    }, function (err) {
				        locales.resolve(defaultLocales);
				    });
                });
			
			return locales.promise();
		},
		exist: function ( name ){
			return name === null ? true: files.hasOwnProperty(name.toLowerCase());
		},
        load: function ( name ){
			
			name = name === null ? "__null__" : name.toLowerCase();
			if(!files.hasOwnProperty(name))
				files[name] = $.Deferred();
			
			if(locale && files[name].state() === "pending")
				_loadFile(name);
			
			return files[name].promise();
        },
        word: function ( key ){
			
			return (typeof key !== "string" || !key) ? "???" : (dictionary[key] || key);
        }
    }
}()), L = Languages.word;

var UI = (function () {
	var iSidebarMenu;
	
	function _create (){

		iSidebarMenu = $("#sea_main_menu").sidebarmenu().sidebarmenu("instance");
	}
	
    return {
		report: function ( msg, originalError, type ){
			console.log((type ? type +": " : "") + msg + (originalError ? " <" + originalError + ">" : ""));
		},
        create: function (){
			
			Languages.load("sea-ui").always(_create);
        }
    }
}());

$(function () {
	Hub.start()
		.then(function () {
			
			Languages.getDataLocale().then(function ( locale ){
				
				if($.type(locale) !== "string" || !locale){
					
					locale = "en";
					Languages.setDataLocale(locale);
				}
				Languages.locale(locale);
				UI.create();
			},function(){
				
				Languages.locale("en");
				UI.create();
			});
		}, function () {
			
			alert("SEA HUB not found!");
		});
})

