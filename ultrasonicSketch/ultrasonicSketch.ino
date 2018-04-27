/*
   Posted on http://randomnerdtutorials.com
   created by http://playground.arduino.cc/Code/NewPing
*/

#include <NewPing.h>

#define TRIGGER_PIN_R 13
#define TRIGGER_PIN_G 11
#define TRIGGER_PIN_B 12
#define TRIGGER_PIN_ROW 10

#define ECHO_PIN_R 5
#define ECHO_PIN_G 3
#define ECHO_PIN_B 4
#define ECHO_PIN_ROW 2

#define MAX_DISTANCE 200

const int numOfReadings = 10;
const int numOfSensors = 4;

int readings[numOfSensors][numOfReadings];
int readingIndexes[numOfSensors] = { 0, 0, 0, 0 };
int readingSums[numOfSensors] = { 0, 0, 0, 0 };


int prevRGBvalues[3] = { 0, 0, 0 };

NewPing sonarR(TRIGGER_PIN_R, ECHO_PIN_R, MAX_DISTANCE); // NewPing setup of pins and maximum distance.
NewPing sonarG(TRIGGER_PIN_G, ECHO_PIN_G, MAX_DISTANCE); // NewPing setup of pins and maximum distance.
NewPing sonarB(TRIGGER_PIN_B, ECHO_PIN_B, MAX_DISTANCE); // NewPing setup of pins and maximum distance.
NewPing sonarRow(TRIGGER_PIN_ROW, ECHO_PIN_ROW, MAX_DISTANCE); // NewPing setup of pins and maximum distance.

unsigned long long prevMillis = 0;

void ClearReadings()
{
  for (int i = 0; i < numOfSensors; i++)
  {
    for (int j = 0; j < numOfReadings; j++)
    {
      readings[i][j] = 0;
    }
  }
}

void setup() {
  Serial.begin(9600);

  pinMode(8, OUTPUT);
  pinMode(7, OUTPUT);
  pinMode(6, OUTPUT);

  ClearReadings();
}

int readSensor(int sensorIndex, int value)
{
  readingSums[sensorIndex] = readingSums[sensorIndex] - readings[sensorIndex][readingIndexes[sensorIndex]];

  readings[sensorIndex][readingIndexes[sensorIndex]] = value;
  readingSums[sensorIndex] = readingSums[sensorIndex] + value;

  readingIndexes[sensorIndex]++;

  if (readingIndexes[sensorIndex] >= numOfReadings) {
    // ...wrap around to the beginning:
    readingIndexes[sensorIndex] = 0;
  }

  return readingSums[sensorIndex] / numOfReadings;
}

int remapValue(int value, int low, int high)
{
  if (value < 10)
  {
    value = 10;
  }
  else if (value > 50)
  {
    value = 50;
  }

  return map(value, 10, 50, low, high);
}

int checkRGBvalues(int index, int value)
{
  int result = value;
  if (abs(prevRGBvalues[index] - value) > 20)
  {
    result = (prevRGBvalues[index] + value) / 2;
  }

  prevRGBvalues[index] = result;
  return result;
}

void loop() {
  int valueR = remapValue(readSensor(0, sonarR.ping_cm()), 0, 255);
  int valueG = remapValue(readSensor(1, sonarG.ping_cm()), 0, 255);
  int valueB = remapValue(readSensor(2, sonarB.ping_cm()), 0, 255);
  int result = remapValue(readSensor(3, sonarRow.ping_cm()), 0, 125);


  if (millis() - prevMillis > 300)
  {
    String stringToSend = "#";
    stringToSend += result;
    stringToSend += ";";
    stringToSend += valueR;
    stringToSend += ";";
    stringToSend += valueG;
    stringToSend += ";";
    stringToSend += valueB;
    stringToSend += "%";
    Serial.println(stringToSend);

    prevMillis = millis();
  }


}
