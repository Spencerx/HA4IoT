﻿appConfiguration.controllerAddress = "192.168.1.15";
appConfiguration.pollInterval = 250;
appConfiguration.showSensorsOverview = true;
appConfiguration.showRollerShuttersOverview = true;

// Supported values for an actuator configuration:
// caption: "The caption (the id is default)
// image: "The name of the image"
// sortValue: 1 (The value which is used to sort the actuators) 
appConfiguration.actuatorExtender = function (actuator) {
    switch (actuator.id) {
        case "Bedroom.Fan":
            {
                actuator.image = "Fan";
                actuator.displayVertical = true;
                break;
            }

        case "Bedroom.SocketWindowLeft":
            {
                actuator.image = "Poison";
                break;
            }

        case "Office.CombinedCeilingLights":
            {
                actuator.displayVertical = true;
                actuator.image = "Lamp";
                break;
            }

        case "UpperBathroom.Fan":
            {
                actuator.image = "Air";
                break;
            }

        case "Storeroom.CirculatingPump":
            {
                actuator.image = "WaterPump";
                break;
            }

        case "Storeroom.CatLitterBoxFan":
            {
                actuator.image = "Cat";
                break;
            }

        case "Office.RemoteSocketDesk":
            {
                actuator.image = "Wifi";
                break;
            }
    }
};

appConfiguration.roomExtender = function (room) {
}

friendlyNameLookup = [
    { key: "Off", value: "Aus" },
    { key: "On", value: "An" },
    { key: "RollerShutter", value: "Rollo" },

    { key: "Bedroom", value: "Schlafzimmer" },
    { key: "LivingRoom", value: "Wohnzimmer" },
    { key: "Office", value: "Büro" },
    { key: "Kitchen", value: "Küche" },
    { key: "Floor", value: "Flur / Treppenhaus" },
    { key: "ChildrensRoom", value: "Kinderzimmer" },
    { key: "ReadingRoom", value: "Lesezimmer" },
    { key: "UpperBathroom", value: "Badezimmer (oben)" },
    { key: "LowerBathroom", value: "Badezimmer (unten)" },
    { key: "Storeroom", value: "Abstellkammer" },

    { key: "Bedroom.RollerShutterLeft", value: "Rollo / links" },
    { key: "Bedroom.RollerShutterRight", value: "Rollo / rechts" },
    { key: "Bedroom.Fan", value: "Ventilator" },
    { key: "Bedroom.SocketWindowLeft", value: "Mückenstecker" },
    { key: "Bedroom.LightCeiling", value: "Licht" },
    { key: "Bedroom.LightCeilingWindow", value: "Strahler / Fenster" },
    { key: "Bedroom.LightCeilingWall", value: "Strahler / Wand" },
    { key: "Bedroom.SocketWindowRight", value: "LED-Band" },
    { key: "Bedroom.SocketWall", value: "Wand" },
    { key: "Bedroom.SocketWallEdge", value: "Ecke" },
    { key: "Bedroom.SocketBedLeft", value: "Bett / links" },
    { key: "Bedroom.SocketBedRight", value: "Bett / rechts" },
    { key: "Bedroom.LampBedRight", value: "Bett / rechts" },
    { key: "Bedroom.LampBedLeft", value: "Bett / links" },

    { key: "Office.LightCeilingFrontLeft", value: "Fenster links" },
    { key: "Office.LightCeilingFrontMiddle", value: "Fenster mitte" },
    { key: "Office.LightCeilingFrontRight", value: "Fenster rechts" },
    { key: "Office.LightCeilingMiddleLeft", value: "Mitte links" },
    { key: "Office.LightCeilingMiddleMiddle", value: "Mitte" },
    { key: "Office.LightCeilingMiddleRight", value: "Mitte rechts" },
    { key: "Office.LightCeilingRearLeft", value: "Schrank" },
    { key: "Office.LightCeilingRearRight", value: "Über Couch" },
    { key: "Office.SocketWindowLeft", value: "Fenster links" },
    { key: "Office.SocketWindowRight", value: "Fenster rechts" },
    { key: "Office.CombinedCeilingLights", value: "Vorlage" },
    { key: "Office.SocketFrontLeft", value: "Schräge links" },
    { key: "Office.SocketFrontRight", value: "Schräge rechts" },
    { key: "DeskOnly", value: "Schreibtisch" },
    { key: "CouchOnly", value: "Couch" },
    { key: "Office.RemoteSocketDesk", value: "Funksteckdose Schreibtisch" },
    { key: "Office.SocketRearLeft", value: "Wand links" },
    { key: "Office.SocketRearLeftEdge", value: "Wand links - Ecke" },
    { key: "Office.SocketRearRight", value: "Wand rechts" },

    { key: "ReadingRoom.LightCeilingMiddle", value: "Licht" },
    { key: "ReadingRoom.SocketWallLeft", value: "Schräge links" },
    { key: "ReadingRoom.SocketWallRight", value: "Schräge rechts" },
    { key: "ReadingRoom.SocketWindow", value: "Fenster" },
    
    { key: "ChildrensRoom.LightCeilingMiddle", value: "Licht" },
    { key: "ChildrensRoom.SocketWallLeft", value: "Schräge links" },
    { key: "ChildrensRoom.SocketWallRight", value: "Schräge rechts" },
    { key: "ChildrensRoom.SocketWindow", value: "Fenster" },

    { key: "UpperBathroom.LightCeilingDoor", value: "Tür" },
    { key: "UpperBathroom.LightCeilingEdge", value: "Ecke" },
    { key: "UpperBathroom.LightCeilingMirrorCabinet", value: "Über Spiegelschrank" },
    { key: "UpperBathroom.LampMirrorCabinet", value: "Spiegelschrank" },
    { key: "UpperBathroom.Fan", value: "Abzug" },

    { key: "LowerBathroom.LightCeilingDoor", value: "Tür" },
    { key: "LowerBathroom.LightCeilingMiddle", value: "Mitte" },
    { key: "LowerBathroom.LightCeilingWindow", value: "Fenster" },
    { key: "LowerBathroom.LampMirror", value: "Spiegel" },

    { key: "Storeroom.LightCeiling", value: "Licht" },
    { key: "Storeroom.CatLitterBoxFan", value: "Katzenklo-Lüftung" },
    { key: "Storeroom.CirculatingPump", value: "Zirkulationspumpe" },

    { key: "Kitchen.LightCeilingDoor", value: "Strahler / Tür" },
    { key: "Kitchen.LightCeilingMiddle", value: "Licht" },
    { key: "Kitchen.LightCeilingPassageInner", value: "Durchgang / innen" },
    { key: "Kitchen.LightCeilingPassageOuter", value: "Durchgang / außen" },
    { key: "Kitchen.LightCeilingWall", value: "Strahler / Wand" },
    { key: "Kitchen.LightCeilingWindow", value: "Strahler / Fenster" },
    { key: "Kitchen.SocketWall", value: "Wand" }
];