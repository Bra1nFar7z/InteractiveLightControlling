#include <UnoWiFiDevEd.h>

#define TOPIC_IN ""
#define TOPIC_OUT ""

#define CONNECTOR "mqtt" // CONNECTOR used with the Ciao library, which is used to communicate with the MQTT broker.
#define LEDPIN 13

String str = "";
bool reading;

void setup() {
  Serial.begin(9600); // Initialize Serial communication.
  Wifi.begin();       // Initialize Wifi communication.
  delay(100);         // Delay is needed to have Wifi initialized.

  Ciao.begin(); // Library used for communicationt to MQTT.
  Serial.begin(9600); // Set speed of serial port for communication to your laptop).
}

void loop() {
  // For debugging, data received from the Serial Monitor is also published it to the MQTT broker.
  if (Serial.available() > 0) {
    char readChar = Serial.read();

    switch (readChar)
    {
      case '#' :
        reading = true;
        break;
      case '%' :
        reading = false;
        if (str != "")
        {
          sendToMQTT(str);
          str = "";
          Serial.flush();
        }
        break;
      default :
        if (reading)
        {
          str += readChar;
        }
        break;
    }
  }
}


void sendToMQTT(String str)
{
  Ciao.write(CONNECTOR, TOPIC_OUT, str); // publishes str to TOPIC_OUT
  Serial.println("Published: " +  str);
}

