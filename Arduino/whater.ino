const int ledBrightnessStep = 2;
const int ledMillsStep = 100;
const int ledMaxValue = 255;

//const int waterMeterCold1 = 4;
//const int waterMeterHot1 = 8;
//const int waterMeterCold2 = 12;
//const int waterMeterHot2 = 13;

//const int ledNetworkPin = 2;

const int NUMBER_OF_INPUTS = 4;

int waterMetersPins[NUMBER_OF_INPUTS] = {
  4, 8, 12, 13
};

int ledPins[NUMBER_OF_INPUTS] = {
  3, 5, 6, 9
};

int waterMetersValues[NUMBER_OF_INPUTS] = { 0, 0, 0, 0 };
int currentWaterMeter = 0;

int ledsBrightness[NUMBER_OF_INPUTS] = { 0, 0, 0, 0 };
int currentLedBrightness = 0;

unsigned long currentMillis = 0;

void setup() {

  Serial.begin(9600);
  //pinMode(ledNetworkPin, OUTPUT);
  //digitalWrite(ledNetworkPin, LOW);
  
  for(int i = 0; i < NUMBER_OF_INPUTS; i++) {
    pinMode(ledPins[i], OUTPUT);
    pinMode(waterMetersPins[i], INPUT);
    digitalWrite(ledPins[i], ledsBrightness[i]);

    waterMetersValues[i] = digitalRead(waterMetersPins[i]);
  }
}

void loop() {

  currentMillis = millis();
  
  for(int i = 0; i < NUMBER_OF_INPUTS; i++) {
    currentLedBrightness = ledsBrightness[i];
      if (currentLedBrightness > 0 && currentMillis % ledMillsStep == 0) {
        currentLedBrightness = currentLedBrightness - ledBrightnessStep;
        if (currentLedBrightness < 0) {
          currentLedBrightness = 0;
        }
        ledsBrightness[i] = currentLedBrightness;
        analogWrite(ledPins[i], currentLedBrightness);
      }

    currentWaterMeter = digitalRead(waterMetersPins[i]);
    if (currentWaterMeter != waterMetersValues[i]) {
      waterMetersValues[i] = currentWaterMeter;
      ledsBrightness[i] = ledMaxValue;
      analogWrite(ledPins[i], ledsBrightness[i]);
      Serial.println(i+1);
     }
  }
}
