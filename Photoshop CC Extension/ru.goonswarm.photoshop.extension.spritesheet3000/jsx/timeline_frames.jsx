/*jslint vars: true, plusplus: true, devel: true, nomen: true, regexp: true, indent: 4, maxerr: 50 */
/*global $, Folder*/
var SPRITE_SHEET_3000_VERSION = 1;

function exportToJSON(exportInfoJson)
{    
    var exportInfo = JSON.parse(exportInfoJson);
    var imageFormat = exportInfo.imageFormat;
    var filterMode = exportInfo.filterMode;
    
    var destFolder = Folder.selectDialog("Please select folder to process");
    if (destFolder == null)
    {
        alert("Target folder not defined")
        return null;
    }
    var destFolderName = destFolder.fullName;
    
    var doc = activeDocument;
    
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
    
    var res = {};
    
    var header = {}
    res["header"] = header;
    
    var frames = []
    res["frames"] = frames;
    
    header["photoshopVersion"] = app.version;
    header["formatVersion"] = SPRITE_SHEET_3000_VERSION;
    header["exportFilterMode"] = filterMode;

    var frameIndex = 0;
    while (frameIndex < 1000)
    {
        try
        {
            var frameId = frameIndex + 1;
            goToFrame(frameId);

            var playbackTime = getFramePropertyDouble(frameId, "animationFrameDelay");
            var layer = getVisibleLayer(doc);
            
            var fileext = ".unknown";
            if (imageFormat == "png8" || imageFormat == "png24")
            {
                fileext = ".png";
            }
            else if (imageFormat == "jpeg")
            {
                fileext = ".jpg";
            }
            
            var filename = layer.name + fileext;
            var saveFile = getOrCreateSaveFile(destFolderName, filename);
            
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
            frame["filename"] = filename;
            frame["playbackTime"] = playbackTime;

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
    saveTextByDocumentName(destFolderName, json);
    
    alert("Export success, ready for use in Spritesheet 3000");
    return json;
}

function saveTextByDocumentName(destinationFolder, txt)
{
    var filename = app.activeDocument.name.replace(/\.[^\.]+$/, '');
    var fileext = decodeURI(app.activeDocument.name).replace(/^.*\./,'');
    if (fileext.toLowerCase() != 'psd') return;

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

function getVisibleLayer(doc)
{
    for (var i = 0; i < doc.artLayers.length; i++) 
    {
        var layer = doc.artLayers[i];
        if (layer.visible)
        {
            return layer;
        }
    }
    return null;
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