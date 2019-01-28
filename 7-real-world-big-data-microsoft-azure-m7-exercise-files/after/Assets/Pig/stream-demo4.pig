register './lib/json-simple-1.1.1.jar';
register './lib/elephant-bird-core-4.6.jar';
register './lib/elephant-bird-pig-4.6.jar';
register './lib/elephant-bird-hadoop-compat-4.6.jar';
register './lib/slf4j-api-1.7.10.jar';

A = LOAD 'F:/device-events/2015/02/01/*' using com.twitter.elephantbird.pig.load.JsonLoader() as (json:map[]);
B = FOREACH A GENERATE json#'eventName' AS eventName, json#'sourceId' as sourceId, json#'timestamp' as timestamp;
C = FILTER B by eventName == 'device.bluetooth.enabled' OR eventName == 'device.bluetooth.disabled';
D = STREAM C through `f:\\temp\\pigstream\\PigStream.exe`;
E = GROUP C ALL;
F = FOREACH E GENERATE COUNT(C);
DUMP F;