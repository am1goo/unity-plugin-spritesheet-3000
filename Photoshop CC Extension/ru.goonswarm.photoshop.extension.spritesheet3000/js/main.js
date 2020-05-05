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

		$("#btn_export_to_json").click(
            function () 
            {
                var ddl_export_image_format = document.getElementById("ddl_export_image_format");
                var ddl_export_image_format_index = ddl_export_image_format.selectedIndex;
                var ddl_export_image_format_value = ddl_export_image_format.options[ddl_export_image_format_index].value;
                var ddl_export_image_format_text = ddl_export_image_format.options[ddl_export_image_format_index].text;

                var ddl_export_filter_mode = document.getElementById("ddl_export_filter_mode");
                var ddl_export_filter_mode_index = ddl_export_filter_mode.selectedIndex;
                var ddl_export_filter_mode_value = ddl_export_filter_mode.options[ddl_export_filter_mode_index].value;
                var ddl_export_filter_mode_text = ddl_export_filter_mode.options[ddl_export_filter_mode_index].text;
				
				var ddl_export_importer_compression = document.getElementById("ddl_export_importer_compression");
                var ddl_export_importer_compression_index = ddl_export_importer_compression.selectedIndex;
                var ddl_export_importer_compression_value = ddl_export_importer_compression.options[ddl_export_importer_compression_index].value;
                var ddl_export_importer_compression_text = ddl_export_importer_compression.options[ddl_export_importer_compression_index].text;
				
				var ddl_export_pixels_per_unit = document.getElementById("ddl_export_pixels_per_unit");
                var ddl_export_pixels_per_unit_value = ddl_export_pixels_per_unit.value;
                
                var exportInfo = {}
                exportInfo["imageFormat"] = ddl_export_image_format_text.toLowerCase();
                exportInfo["filterMode"] = ddl_export_filter_mode_text;
				exportInfo["importerCompression"] = ddl_export_importer_compression_text;
				exportInfo["pixelsPerUnit"] = ddl_export_pixels_per_unit_value;
				
                var exportInfoJson = JSON.stringify(exportInfo).replace(/"/g,'\\"');
                csInterface.evalScript('exportToJSON("' + exportInfoJson +'")', function(result) 
                {
                });
            });
    }

    init();


}());