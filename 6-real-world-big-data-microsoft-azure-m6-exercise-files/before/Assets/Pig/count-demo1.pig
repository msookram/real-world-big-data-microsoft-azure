

A = LOAD 'F:/device-events/2015/02/01/*';
B = GROUP A all;
C = FOREACH B GENERATE COUNT(A);
DUMP C;