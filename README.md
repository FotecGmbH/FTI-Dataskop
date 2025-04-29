# Dataskop Backend <img src="https://github.com/user-attachments/assets/8ea5a52b-942e-4315-aed5-c8bf718ad683" width="50"/>

Das Forschungsprojekt "Dataskop", hat das Ziel, mithilfe von situativ eingebetteten Datenvisualisierungsmethoden, die speziell für den Einsatz im ländlichen Raum gedacht sind, unsichtbare Umweltdaten für Menschen sichtbar zu machen. 
Nähere Informationen zu dem Projekt finden Sie hier: [Dataskop](https://dataskop.fhstp.ac.at/).
In diesem Repository befindet sich das hierfür entwickelte Datenmanagementsystem.
Dieses ist modular strukturiert und durch seine offene Architektur flexibel und zukunftssicher aufgebaut. 

<img src="https://github.com/user-attachments/assets/9b1e37d5-71b7-4695-a1a5-9a32b1ec41f2" width="100"/>
<img src="https://github.com/user-attachments/assets/da7651e6-291f-40f3-8e93-33e5e62d4a05" width="100"/>
<img src="https://github.com/user-attachments/assets/68880647-492f-4114-af31-7dc4afa74218" width="100"/>
<img src="https://github.com/user-attachments/assets/22904d0e-642f-42cf-955f-a364d3d639ad" width="100"/>


<br>
<br>

## Komponenten-Struktur
<img src="https://github.com/user-attachments/assets/f42bfa05-a8f0-4fe5-ba9b-6e9494861424" width="500"/>

## Repository-Struktur

### BDA.Common
Allgemeine Bibliotheken welche für alle Projekte als gemeinsame Basis / Austausch dienen

- Exchange - für alle Projekte
- WebExchange - für alle Projekte in BDA.Service
- BDA.Common.ConfigMaker - Erstellung für die dynamischen Konfigurationen in der Datenbank
- BDA.CliApp - Basis für CLI Apps 
- BDA.Common.TtnClient - Basis um bei TTN Iot Devices automatisiert anzulegen

### BDA.Service
Hauptservice des BDA Backend. Darin enhalten ist die Kommunikation mit "außen", den Gateways und der BDA Backend App. Inklusive der Backend Apps.

- Apps
	- Blazor
		- BlazorApp - BDA Backend App für Blazor WASM
	- Xamarin
		- AndroidApp - BDA Backend App für Android
		- IOsApp - BDA Backend App für iOs
		- WpfApp - BDA Backend App für Windows
	- BaseApp - ViewModel für alle BDA Backend Apps
	- BaseApp.View.Xamarin - Views für alle Xamarin BDA Backend Apps
- Cloud
	- ConnectivityHost - Hauptservice des BDA Backend
	- Database - BDA Backend Datenbank
	- BDA.GatewayService - Kommunikationsschicht für die Kommunikation mit den Gateways
	- BDA.Service.AppConnectivity - Kommunikation (DC) mit den BDA Backend Apps
	- BDA.Service.TriggerAgent - Service für aktive Weiterleitung von Daten	
	
Com	
	- BDA.Service.Com.Base - Basis für die Kommunikation mit der "Außenwelt"
	- BDA.Service.Com.Rest - Kommunikation via REST

### BDA.Gateway
Hauptapplikation eines einzelnen Gateway. Dieser nimmt Daten von Iot-Geräten entgegen und leitet diese an den Server weiter.

- BDA.Gateway.App - Gateway-App - Als Console App
- BDA.Gateway.Core - Gateway-Core - Basis für alle (zukünftigen Gatway Apps - zurzeit "nur" für die ConsoleApp)
- Com - wie wird mit einem Gateway kommuniziert
	- BDA.Gateway.Com.Base - Leitet Daten an das Backend weiter (zwischenspeichert diese bei Bedarf)
	- BDA.Gateway.Com.Serial - Daten werden über das serielle Port übertragen (leitet von BDA.Gateway.Com.Base ab)
	- BDA.Gateway.Com.Tcp - Daten werden über Tcp übertragen (leitet von BDA.Gateway.Com.Base ab)
	- BDA.Gateway.Com.Ttn - Daten werden über TheThingsNetwork übertragen (leitet von BDA.Gateway.Com.Base ab)
	- BDA.Gateway.Com.IotInGateway - Daten werden über ein in das Gatway integriertes Iot Device übertragen (leitet von BDA.Gateway.Com.Base ab)

### BDA.IotDevice
Hauptapplikation eines einzelnen DotNet basierenden Iot-Devices. Dieser leiten Daten an einen Gateway oder ein Drittsystem (von welem dann ein GW die Daten bekommt) weiter.
- BDA.IotDevice.App - IotDevice-App - Als Console App
- BDA.IotDevice.AppPi - IoTDevice-App - Als Console App für einen Raspberry Pi (DotNet Basis)
- BDA.IotDevice.Core - Gateway-Core - Basis für alle (zukünftigen Gatway Apps - zurzeit "nur" für die ConsoleApp)
- Com.Downstream - Wie wird mit Sensoren kommuniziert
	- BDA.IotDevice.Com.Downstream.Base - Basiskommunikation mit Sensoren
	- BDA.IotDevice.Com.Downstream.I2C - Kommunikation mit I2C Sensoren (leitet von BDA.IotDevice.Com.Downstream.Base ab)
	- BDA.IotDevice.Com.Downstream.DotNet - Ein auf Dotnet basierender Sonsor - wobei Sonsor == IotDevice ist (leitet von BDA.IotDevice.Com.Downstream.Base ab)
	- BDA.IotDevice.Com.Downstream.Pi - Ein auf einem Rasperry Pi basierender Sonsor - wobei Sonsor == IotDevice ist (leitet von BDA.IotDevice.Com.Downstream.DotNet ab)

Com.Upstream - Wie wird mit Gateways kommuniziert
	- BDA.IotDevice.Com.Upstream.Base - Basiskommunikation mit einem Gateway
	- BDA.IotDevice.Com.Upstream.IotInGateway - IoT Device wird direkt im Gatway ausgeführt "Embeddet" - "Gateway AS IotDevice" (leitet von BDA.IotDevice.Com.Upstream.Base ab)
	- BDA.IotDevice.Com.Upstream.Serial - Kommunikation via serielle Schnittstelle (leitet von BDA.IotDevice.Com.Upstream.Base ab)
	- BDA.IotDevice.Com.Upstream.Tcp - Kommunikation via Ethernet Tcp (leitet von BDA.IotDevice.Com.Upstream.Base ab)

