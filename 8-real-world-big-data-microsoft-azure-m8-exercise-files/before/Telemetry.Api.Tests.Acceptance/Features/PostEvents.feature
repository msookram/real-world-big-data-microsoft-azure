Feature: Telemetry Ingestion
	In order to record device usage
	As a Data Scientist
	I want to ingest telemetry events 

Scenario: single event ingestion
	Given a device with id 'x959e73c063016dv54c93d2d86706c2f' has recorded an event payload
	And the event content is from 'device-event.json'
	When the device reports the event	
	Then the payload is sent to the Telemetry API
	And the API relays 1 event to the event ingestion API
	And the ingestion API records an event with the expected Fields and Values
	| Field       | Value                                                            |
	| deviceId    | x959e73c063016dv54c93d2d86706c2f                                 |
	| eventName   | device.logs.message                                              |
	| dmVersion   | 1.4.4                                                            |
	| id          | 76c8babb519d8e95fcdc1dc7200cbdb1c6ff146a5ff091d32b7d606e274b79a7 |
	| deviceModel | Tablet 1                                                         |
	And the device receives an API response
	And the response status code is '201'

Scenario: no event ingestion
	Given a device with id 'x959e73c063016dv54c93d2d86706c2f' has recorded an event payload
	And the event content is from 'no-event.json'
	When the device reports the event	
	Then the payload is not sent to the Telemetry API	
	And the device receives an API response
	And the response status code is '200'

Scenario: compressed event ingestion
	Given a device with id 'x959e73c063016dv54c93d2d86706c2f' has recorded an event payload
	And the event content is from 'device-event-compressed.json.gz'
	When the device reports the event	
	Then the payload is sent to the Telemetry API
	And the API relays 1 event to the event ingestion API
	And the ingestion API records an event with the expected Fields and Values
	| Field       | Value                                                            |
	| deviceId    | x959e73c063016dv54c93d2d86706c2f                                 |
	| eventName   | device.logs.message                                              |
	| dmVersion   | 1.4.4                                                            |
	| id          | 76c8babb519d8e95fcdc1dc7200cbdb1c6ff146a5ff091d32b7d606e274b79a7 |
	| deviceModel | Tablet 1                                                         |
	And the device receives an API response
	And the response status code is '201'

Scenario: large compressed event ingestion
	Given a device with id '1e56548457175f74de89632b400212a2' has recorded an event payload
	And the event content is from 'device-events-large.json.gz'
	When the device reports the event	
	Then the payload is sent to the Telemetry API
	And the API relays 1162 event to the event ingestion API
	And the ingestion API records the first event with the expected Fields and Values
	| Field     | Value                            |
	| deviceId  | 1e56548457175f74de89632b400212a2 |
	| eventName | device.activated                 |
	| dmVersion | 1.4.4                            |
	| timestamp | 1415807506426                    |
	And the ingestion API records the last event with the expected Fields and Values
	| Field     | Value                            |
	| deviceId  | 1e56548457175f74de89632b400212a2 |
	| eventName | device.wifi.connected            |
	| BSSID     | 06:18:1a:82:c8:16                |
	| timestamp | 1415886409417                    |
	And the device receives an API response
	And the response status code is '201'