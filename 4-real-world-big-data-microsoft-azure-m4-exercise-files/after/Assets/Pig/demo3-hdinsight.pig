register 'wasb:///lib/json-simple-1.1.1.jar';
register 'wasb:///lib/elephant-bird-core-4.6.jar';
register 'wasb:///lib/elephant-bird-pig-4.6.jar';
register 'wasb:///lib/elephant-bird-hadoop-compat-4.6.jar';
register 'wasb:///lib/slf4j-api-1.7.10.jar';

A = LOAD 'wasb://device-events@devicetelemetryprd.blob.core.windows.net/2015/03/*/*' USING com.twitter.elephantbird.pig.load.JsonLoader() as (json:Map[]);
B = FOREACH A GENERATE json#'eventName' AS eventName;
C = GROUP B BY eventName PARALLEL 4;
D = FOREACH C GENERATE group, COUNT(B);
STORE D INTO 'event-count-201503.tsv';
