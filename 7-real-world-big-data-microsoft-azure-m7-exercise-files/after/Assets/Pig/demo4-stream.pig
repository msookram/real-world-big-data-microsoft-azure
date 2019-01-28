register 'wasb:///lib/json-simple-1.1.1.jar';
register 'wasb:///lib/elephant-bird-core-4.6.jar';
register 'wasb:///lib/elephant-bird-pig-4.6.jar';
register 'wasb:///lib/elephant-bird-hadoop-compat-4.6.jar';
register 'wasb:///lib/slf4j-api-1.7.10.jar';

DEFINE X `Telemetry.EventProcessor.Pig.CountLogger.exe` ship('c:\\piglogger\\Telemetry.EventProcessor.Pig.CountLogger.exe','c:\\piglogger\\Telemetry.EventProcessor.Pig.CountLogger.exe.config', 'c:\\piglogger\\nlog-prd.config', 'c:\\piglogger\\Microsoft.WindowsAzure.Configuration.dll', 'c:\\piglogger\\NLog.dll', 'c:\\piglogger\\Telemetry.Core.dll', 'c:\\piglogger\\Newtonsoft.Json.dll');

A = LOAD 'wasb://device-events@devicetelemetryprd.blob.core.windows.net/2015/03/31/*/*' USING com.twitter.elephantbird.pig.load.JsonLoader() as (json:map[]);
B = FOREACH A GENERATE json#'eventName' AS eventName, json#'sourceId' as sourceId, json#'timestamp' as timestamp;
C = STREAM B through X;
STORE C INTO 'streamed-event-count-20150331.tsv';
