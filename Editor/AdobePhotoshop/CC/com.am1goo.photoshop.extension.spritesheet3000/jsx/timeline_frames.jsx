/*jslint vars: true, plusplus: true, devel: true, nomen: true, regexp: true, indent: 4, maxerr: 50 */
/*global $, Folder*/
var SPRITE_SHEET_3000_VERSION = 3;
var KEY_PREVIOUS_DEST_FOLDER = "spritesheet3000_key_previous_dest_folder";

function exportToJSON(exportInfoJson)
{   
    var filename = app.activeDocument.name.replace(/\.[^\.]+$/, '');
    var fileext = decodeURI(app.activeDocument.name).replace(/^.*\./,'');
    if (fileext.toLowerCase() != 'psd')
	{
		alert("This file is not a 'PSD' file");
		return;
	}
 
    var exportInfo = JSON.parse(exportInfoJson);
    var imageFormat = exportInfo.imageFormat;
    var filterMode = exportInfo.filterMode;
	var importerCompression = exportInfo.importerCompression;
	var pixelsPerUnit = exportInfo.pixelsPerUnit;
	var spriteMeshType = exportInfo.spriteMeshType;
	var spritePivot = exportInfo.spritePivot;
	
    var tempFolderName = getTempOptions(KEY_PREVIOUS_DEST_FOLDER, "C:\\");

	var destFolder = Folder(tempFolderName).selectDlg("Please select folder to process");
    if (destFolder == null)
    {
        alert("Target folder not defined")
        return null;
    }
	
    var destFolderName = destFolder.fullName;
	saveTempOptions(KEY_PREVIOUS_DEST_FOLDER, destFolderName);
	
    var requiredUndo = false;
    try
    {
        executeAction( stringIDToTypeID( "convertTimeline" ), new ActionDescriptor(), DialogModes.NO );
        requiredUndo = true;
    }
    catch(e)
    {
        requiredUndo = false;
        //already converted if exception
    }
    	
    var doc = activeDocument;
    var res = {};
    
    var header = {}
    res["header"] = header;
    
    var frames = []
    res["frames"] = frames;
	
	var spritePivot2 = {}
	
	if (spritePivot == "Center")
	{
		spritePivot2["x"] = 0.5;
		spritePivot2["y"] = 0.5;
	}
	else if (spritePivot == "Top Left")
	{
		spritePivot2["x"] = 0;
		spritePivot2["y"] = 1;
	}
	else if (spritePivot == "Top")
	{
		spritePivot2["x"] = 0.5;
		spritePivot2["y"] = 1;
	}
	else if (spritePivot == "Top Right")
	{
		spritePivot2["x"] = 1;
		spritePivot2["y"] = 1;
	}
	else if (spritePivot == "Left")
	{
		spritePivot2["x"] = 0;
		spritePivot2["y"] = 0.5;
	}
	else if (spritePivot == "Right")
	{
		spritePivot2["x"] = 1;
		spritePivot2["y"] = 0.5;
	}
	else if (spritePivot == "Bottom Left")
	{
		spritePivot2["x"] = 0;
		spritePivot2["y"] = 0;
	}
	else if (spritePivot == "Bottom")
	{
		spritePivot2["x"] = 0.5;
		spritePivot2["y"] = 0;
	}
	else if (spritePivot == "Bottom Right")
	{
		spritePivot2["x"] = 1;
		spritePivot2["y"] = 0;
	}
	else 
	{
		spritePivot2["x"] = -1;
		spritePivot2["y"] = -1;
	}
    
    header["photoshopVersion"] = app.version;
    header["formatVersion"] = SPRITE_SHEET_3000_VERSION;
    header["exportFilterMode"] = filterMode;
	header["exportImporterCompression"] = importerCompression;
	header["exportPixelsPerUnit"] = pixelsPerUnit;
	header["exportSpriteMeshType"] = spriteMeshType;
	header["exportSpritePivot"] = spritePivot2;

    var frameIndex = 0;
    while (frameIndex < 1000)
    {
        try
        {
            var frameId = frameIndex + 1;
            goToFrame(frameId);

            var framePlaybackTime = getFramePropertyDouble(frameId, "animationFrameDelay");
            var frameName = getFrameName(filename, frameId);
            
            var fileExt = ".unknown";
            if (imageFormat == "png8" || imageFormat == "png24")
            {
                fileExt = ".png";
            }
            else if (imageFormat == "jpeg")
            {
                fileExt = ".jpg";
            }
            
            var frameFilename = frameName + fileExt;
            var saveFile = getOrCreateSaveFile(destFolderName, frameFilename);
            
            if (imageFormat == "png8")
            {
                savePng8(saveFile);
            }
            else if (imageFormat == "png24")
            {
                savePng24(saveFile);
            }
            else if (imageFormat == "jpeg")
            {
                saveJpeg(saveFile);
            }

            var frame = {};
            frame["filename"] = frameFilename;
            frame["playbackTime"] = framePlaybackTime;

            frames[frameIndex] = frame;
            frameIndex++;
        }
        catch(e)
        {
            break;
        }
    }
    
    if (requiredUndo)
    {
        doc.activeHistoryState = doc.historyStates[doc.historyStates.length-2]; 
    }
    
    var json = JSON.stringify(res);
    saveTextByDocumentName(filename, destFolderName, json);
    
    alert("Export success, ready for use in Spritesheet 3000");
    return json;
}

function getDefaultParamValue()
{ 
   return stringIDToTypeID("default_param_value"); 
}

function hasTempOptions(key)
{
	try
	{
		var desc = getCustomOptions(key);
		return desc != null;
	}
	catch(e)
	{
		return false;
	}
}

function getTempOptions(key, defaultValue)
{
	var has = hasTempOptions(key);
	if (has)
	{
		var desc = app.getCustomOptions(key);
		var str = desc.getString(getDefaultParamValue());
		return str;
	}
	else
	{
		saveTempOptions(key, defaultValue);
		return defaultValue;
	}
}	

function saveTempOptions(key, value)
{
	var desc = new ActionDescriptor();
	desc.putString(getDefaultParamValue(), value, true);
	app.putCustomOptions(key, desc);
}

function saveTextByDocumentName(filename, destinationFolder, txt)
{
    var saveFile = getOrCreateSaveFile(destinationFolder, filename + ".txt");
    saveText(saveFile, txt);
}

function getOrCreateSaveFile(destinationFolder, filename)
{
    var saveFile = File(destinationFolder + "/" + filename);

    if(saveFile.exists)
        saveFile.remove();
    
    return saveFile;
}

function saveText(saveFile, txt)
{
    saveFile.encoding = "UTF8";
    saveFile.open("e", "TEXT", "????");
    saveFile.writeln(txt);
    saveFile.close();
}

function savePng8(saveFile)
{
	savePng(saveFile, true);
}

function savePng24(saveFile)
{
    savePng(saveFile, false);
}

function savePng(saveFile,is8bit)
{
	var pngOpts = new ExportOptionsSaveForWeb;  
    pngOpts.format = SaveDocumentType.PNG  
    pngOpts.PNG8 = is8bit;  
    pngOpts.transparency = true;  
    pngOpts.interlaced = false;  
    pngOpts.quality = 100;  
    activeDocument.exportDocument(saveFile,ExportType.SAVEFORWEB,pngOpts);  
}

function saveJpeg(saveFile) { 
    var jpegSaveOptions = new JPEGSaveOptions(); 
    jpegSaveOptions.embedColorProfile = true;
	jpegSaveOptions.formatOptions = FormatOptions.STANDARDBASELINE;
	jpegSaveOptions.matte = MatteType.NONE;
    activeDocument.saveAs(saveFile, jpegSaveOptions, true, Extension.LOWERCASE); 
} 

function getLayer(doc, layerName)
{
    return doc.artLayers.getByName(layerName);
}

function goToFrame(idx)
{
    var desc = new ActionDescriptor();  
    var ref1 = new ActionReference();  
    ref1.putIndex( stringIDToTypeID( "animationFrameClass" ), idx );  
    desc.putReference( charIDToTypeID( "null" ), ref1 );  
    executeAction( charIDToTypeID( "slct" ), desc, DialogModes.NO ); 
}

function getFramePropertyString(idx, propertyName)
{
    var propertyId = stringIDToTypeID(propertyName)
    var property = getFrameProperty(idx, propertyId);
    return property.getString(propertyId);
}

function getFramePropertyDouble(idx, propertyName)
{
    var propertyId = stringIDToTypeID(propertyName)
    var property = getFrameProperty(idx, propertyId);
    return property.getDouble(propertyId)
}

function getFrameProperty(idx, propertyId)
{
   try 
   {
        var actionReference = new ActionReference(); 
        actionReference.putProperty(charIDToTypeID('Prpr'), propertyId); 
        actionReference.putIndex(stringIDToTypeID('animationFrameClass'), idx);
      
        var actionDescriptor = new ActionDescriptor(); 
        actionDescriptor.putReference(charIDToTypeID('null'), actionReference);          

        return executeAction(charIDToTypeID('getd'), actionDescriptor, DialogModes.NO);
   }
   catch(e) 
   {
        alert(e + ': on line ' + e.line);
        return null;
  }
}

function getFrameName(frameName, frameId)
{
	if (frameId < 10)
	{
		return frameName + "_0" + frameId;
	}
	else
	{
		return frameName + "_" + frameId;
	}
}