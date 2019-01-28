register './lib/json-simple-1.1.1.jar';
register './lib/elephant-bird-core-4.6.jar';
register './lib/elephant-bird-pig-4.6.jar';
register './lib/elephant-bird-hadoop-compat-4.6.jar';
register './lib/slf4j-api-1.7.10.jar';

A = LOAD 'F:/device-events/2015/04/08/*' using com.twitter.elephantbird.pig.load.JsonLoader() as (json:map[]);
B = FOREACH A GENERATE json#'eventName' AS eventName;
C = GROUP B BY eventName;
D = FOREACH C GENERATE group, COUNT(B);
STORE D INTO 'event-count-20150408.tsv';