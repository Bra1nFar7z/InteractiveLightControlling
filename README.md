# InteractiveLightControlling
A small project to control some interactive light on FONTYS in Eindhoven, using ultrasonic sensor


This project incl. a c# application to recieve data from ultrasonic sensor over a MQTT protocol, and then send it over a HTTP Protocol to an API, which controls light strips on FONTYS in Eindhoven.
The c# application can only send to the API, if you are logged in, on their WIFI and their VPN.
