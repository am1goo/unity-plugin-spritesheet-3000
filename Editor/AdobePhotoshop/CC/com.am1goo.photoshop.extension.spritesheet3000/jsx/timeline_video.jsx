/*jslint vars: true, plusplus: true, devel: true, nomen: true, regexp: true, indent: 4, maxerr: 50 */
/*global $, Folder*/
var SPRITE_SHEET_3000_VERSION = 1;

function exportToJSON()
{
    var res = {};
    
    var photoshopVersion = app.version;
    res["photoshopVersion"] = photoshopVersion;
    
    var frameCount = getClassPropertyInteger("timeline", "frameCount");
    res["frameCount"] = frameCount;
    
    var frameRate = getClassPropertyDouble("timeline", "frameRate");
    res["frameRate"] = frameRate;
    
    var frames = [];
    res["frames"] = frames;
    
    var cycle_i = 0;
    while (cycle_i < frameCount)
    {
        goToFrame(cycle_i);
        var layer = app.activeDocument.activeLayer;
        frames[cycle_i] = layer.name;
        cycle_i++;
    }
    alert(JSON.stringify(frameCount));
    goToFrame(0);
    
    return JSON.stringify(res);
}

function goToFrame(frameNumber) 
{
    try {
        var desc1 = new ActionDescriptor();
        var ref1 = new ActionReference();
        ref1.putProperty( charIDToTypeID( "Prpr" ), stringIDToTypeID( "time" ) );
        ref1.putClass( stringIDToTypeID( "timeline" ) );
        desc1.putReference( charIDToTypeID( "null" ), ref1 );
        var desc2 = new ActionDescriptor();
        desc2.putInteger( stringIDToTypeID( "frame" ), frameNumber );
        desc1.putObject( charIDToTypeID( "T   " ), stringIDToTypeID( "timecode" ), desc2 );
        executeAction( charIDToTypeID( "setd" ), desc1, DialogModes.NO );
        return true;
    }
    catch(e) 
    {
        $.writeln(e);
    }
    return false;
}

function getClassProperty(className, propertyId){
    var ref = new ActionReference();    
    ref.putProperty(stringIDToTypeID("property"), propertyId);    
    ref.putClass(stringIDToTypeID(className));   
    return executeActionGet(ref);
}

function getClassPropertyInteger(className, propertyName){
    var propertyId = stringIDToTypeID(propertyName);
    var ret = getClassProperty(className, propertyId);
    return ret.getInteger(propertyId);
}

function getClassPropertyDouble(className, propertyName){
    var propertyId = stringIDToTypeID(propertyName);
    var ret = getClassProperty(className, propertyId);
    return ret.getDouble(propertyId);
}

function getClassPropertyBoolean(className, propertyName){
    var propertyId = stringIDToTypeID(propertyName);
    var ret = getClassProperty(className, propertyId);
    return ret.getBoolean(propertyId);
}

function getClassPropertyActionDescriptor(className, propertyName){
    var propertyId = stringIDToTypeID(propertyName);
    var ret = getClassProperty(className, propertyId);
    return ret.getObjectValue(propertyId);
}