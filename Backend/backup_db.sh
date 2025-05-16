mongoexport --host 221.12.170.99 \
--port 27017 \
--username root \
--password 565c19a1-54ff-4a8f-9b5d-6aab50493463 \
--authenticationDatabase admin \
--db AnimationSource \
--collection Actor --forceTableScan \
--out Actor.json

mongoexport --host 221.12.170.99 \
--port 27017 \
--username root \
--password 565c19a1-54ff-4a8f-9b5d-6aab50493463 \
--authenticationDatabase admin \
--db AnimationSource \
--collection Animation --forceTableScan \
--out Animation.json

mongoexport --host 221.12.170.99 \
--port 27017 \
--username root \
--password 565c19a1-54ff-4a8f-9b5d-6aab50493463 \
--authenticationDatabase admin \
--db AnimationSource \
--collection Preview --forceTableScan \
--out Preview.json

mongoexport --host 221.12.170.99 \
--port 27017 \
--username root \
--password 565c19a1-54ff-4a8f-9b5d-6aab50493463 \
--authenticationDatabase admin \
--db AnimationSource \
--collection UserData --forceTableScan \
--out UserData.json
