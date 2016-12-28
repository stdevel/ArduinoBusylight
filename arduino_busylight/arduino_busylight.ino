#include"tones.h"
#include <CmdMessenger.h>

//kSetLed,Away;
//1,kRing;

//Pin definitions
const int piezoPin = 8;
const int greenLEDPin = 9;
const int redLEDPin = 10;
const int blueLEDPin = 11;

//Colors
const int color_green[3] = { 0, 255, 0 };
const int color_yellow[3] = { 255, 255, 0 };
const int color_red[3] = { 255, 0, 0 };
const int color_purple[3] = { 255, 0, 255 };

//Ringtone
int ringtone_melody[] = {
  NOTE_E3, NOTE_D3, NOTE_FS3, NOTE_GS3,
  NOTE_C3, NOTE_B3, NOTE_D3, NOTE_E3,
  NOTE_B3, NOTE_A3, NOTE_CS3, NOTE_E3,
  NOTE_A3
};
int ringtone_durations[] = {
  8, 8, 4, 4,
  8, 8, 4, 4,
  8, 8, 4, 4,
  1
};

//Runtime definitions
String msg = "";
CmdMessenger cmdMessenger = CmdMessenger(Serial);

//Serial commands
enum {
  kSetLed,    //set LED color
  kRing,      //play sound
  kStatus,    //report status
};

void add_callbacks()
{
  cmdMessenger.attach(kSetLed, set_color);
  cmdMessenger.attach(kRing, play_sound);
  cmdMessenger.sendCmd(kStatus,"Added callbacks");
}

void set_color()
{
  msg = cmdMessenger.readStringArg();
  cmdMessenger.sendCmd(kStatus,msg);

  if(msg == "Busy" || msg == "BusyIdle" ) {
    led_color(color_red[0], color_red[1], color_red[2]);
  }
  else if(msg == "DoNotDisturb") {
    led_color(color_purple[0], color_purple[1], color_purple[2]);
  }
  else if(msg == "Away" || msg == "TemporarilyAway" || msg == "FreeIdle") {
    led_color(color_yellow[0], color_yellow[1], color_yellow[2]);
  }
  else if(msg == "Free") {
    led_color(color_green[0], color_green[1], color_green[2]);
  }
  else
  {
    led_reset();
  }
}

void led_reset()
{
  //Reset LED
  cmdMessenger.sendCmd(kStatus,"Reset LED...");
  //Serial.println("Reset LED");
  analogWrite(redLEDPin, LOW);
  analogWrite(greenLEDPin, LOW);
  analogWrite(blueLEDPin, LOW);
}

void led_color(int r, int g, int b)
{
  //Set LED color
  led_reset();
  cmdMessenger.sendCmd(kStatus,"Setting color...");
  analogWrite(redLEDPin, r);
  analogWrite(greenLEDPin, g);
  analogWrite(blueLEDPin, b);
}

void play_sound()
{
  //Play the tingtone
  cmdMessenger.sendCmd(kStatus,"Playing sound...");
  for (int thisNote = 0; thisNote < 13; thisNote++) {
    // to calculate the note duration, take one second
    // divided by the note type.
    //e.g. quarter note = 1000 / 4, eighth note = 1000/8, etc.
    int noteDuration = 1000 / ringtone_durations[thisNote];
    tone(piezoPin, ringtone_melody[thisNote], noteDuration);

    // to distinguish the notes, set a minimum time between them.
    // the note's duration + 30% seems to work well:
    int pauseBetweenNotes = noteDuration * 1.30;
    delay(pauseBetweenNotes);
    // stop the tone playing:
    noTone(piezoPin);
  }  
}

void setup() {
  //Setup LED pins
  pinMode(greenLEDPin, OUTPUT);
  pinMode(redLEDPin, OUTPUT);
  pinMode(blueLEDPin, OUTPUT);

  //Initialize serial connection
  Serial.begin(9600);
  //Serial.begin(115200);
  //Add newline to each command
  cmdMessenger.printLfCr();
  add_callbacks();
  cmdMessenger.sendCmd(kStatus,"Setup complete, waiting for input...");
}

void loop() {
  //Listen for incoming connections
  cmdMessenger.feedinSerialData();
}
