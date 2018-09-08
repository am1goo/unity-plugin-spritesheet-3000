/*jslint vars: true, plusplus: true, devel: true, nomen: true, regexp: true, indent: 4, maxerr: 50 */
/*global $, window, location, CSInterface, SystemPath, themeManager*/

(function () {
    'use strict';

    var csInterface = new CSInterface();

    function loadJSX (fileName) {
        var extensionRoot = csInterface.getSystemPath(SystemPath.EXTENSION) + "/jsx/";
        csInterface.evalScript('$.evalFile("' + extensionRoot + fileName + '")');
    }
    
    function init() {
    
        themeManager.init();
		loadJSX("json2.js");
   
        var ddl_export_filter_mode = document.getElementById("ddl_export_filter_mode");
        var ddl_export_filter_mode_index = ddl_export_filter_mode.selectedIndex;
        var ddl_export_filter_mode_value = ddl_export_filter_mode.options[ddl_export_filter_mode_index].value;
        var ddl_export_filter_mode_text = ddl_export_filter_mode.options[ddl_export_filter_mode_index].text;

		$("#btn_export_to_json").click(
            function () 
            {
                csInterface.evalScript('exportToJSON(' + JSON.stringify(ddl_export_filter_mode_text) +')', function(json) 
                {
                });
            });
    }

    init();


}());