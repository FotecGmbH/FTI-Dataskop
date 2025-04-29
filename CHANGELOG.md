# BISS Apps  

## Changelog

### Version 1.2.8
REST Controller:
 MeasurementResultController
  (neuer endpunkt) post create simple
Application Insights bugfixes
Mqtt
Drei Passwort Verschluesselung
OData Endpunkte

### Version 1.2.7

Custom Code:
 Support for Multiple Results per definition per message for Wpf, blazor & gateway

### Version 1.2.6

REST Controller:
 MeasurementResultController
  post create measurementResults mit backgroundworker 
  delete measurementResults bugfix

### Version 1.2.5

REST Controller:
 Geo Controller:
  Bugfix arealatest
 MeasurementDefinitionController
  Bugfix latest result

### Version 1.2.4

Trigger Agent NewMeasurementsFromGateway Rueckgabewert "Task"
MeasurementResultController POST await NewMeasurementsFromGateway

### Version 1.2.3

Drei Global config
Drei IoTDevice

Drei Gateway

### Version 1.2.2

BISS Nuget Update =>8.2

REST Controller:
 Geo Controller
  Zusaetzlicher Endpunkt fuer letzte result pro measurementdefinition in gewissen radius

Bugfix VmInfrastructure ObservableCollectionChanged

Bugfix Menu Blazor wenn noch nicht eingelogt AddDefault()

### Version 1.2.1

REST Controller:
 Geo Controller
  Result hat MeasurementDefinitionId und AdditionalProperties

WpfConfig App:
 Bugfix crash on start
 Bugfix Companies dropdown / zu viel wird angezeigt


### Version 1.2.0

BISS Nuget Update

REST Controller:
 IoTDevice Controller
  /api/iotdevice/list
  /api/iotdevice/measurementresults/{id}
 MeasurementResultController
  /api/measurementresult/query/{id}/{take}/{skip}
   kein order by; take + skip; take Begrenzung 5000

.NET IoTDevice Tcp Communication

Diverse Bugfixes

### Version 1.0

