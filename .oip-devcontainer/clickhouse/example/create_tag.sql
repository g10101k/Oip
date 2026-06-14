CREATE TABLE `test.:tes:-+`
(
    `time`   DateTime64(3),
    `value`  Nullable(Float32),
    `status` Enum('Sensor Failure' = -3, 'Device Failure' = -2, 'Bad' = -1, 'Good' = 0 )
)
    ENGINE = MergeTree
        ORDER BY `time`;


ALTER TABLE `test.:tes:-+`
    MODIFY COLUMN status Enum('Sensor Failure' = -3, 'Device Failure' = -2, 'Bad' = -1, 'Good' = 0 );


ALTER TABLE `test.:tes:-+`
    MODIFY COLUMN value Nullable(Float32);


INSERT INTO `test.:tes:-+`
values (now64(3), null, 0);


select *
from `test.:tes:-+`