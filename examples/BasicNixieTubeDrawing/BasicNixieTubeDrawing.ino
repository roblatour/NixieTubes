// Nixie clock (for TFT Display)
//
// Copyright Rob Latour, 2023
// htts://raltour.com
// https://github.com/roblatour/ not yet posted
//
// Compile and upload using Arduino IDE (2.0.3 or greater)
//
// Physical board:                 LILYGO T-Display-S3
// Board in Arduino board manager: ESP32S3 Dev Module
//
// Arduino Tools settings:
// USB CDC On Boot:                Enabled
// CPU Frequency:                  240MHz
// USB DFU On Boot:                Enabled
// Core Debug Level:               None
// Erase All Flash Before Upload:  Disabled
// Events Run On:                  Core 1
// Flash Mode:                     QIO 80Mhz
// Flash Size:                     16MB (128MB)
// Arduino Runs On:                Core 1
// USB Firmware MSC On Boot:       Disabled
// PSRAM:                          OPI PSRAM
// Partition Scheme:               16 M Flash (3MB APP/9.9MB FATFS)
// USB Mode:                       Hardware CDC and JTAG
// Upload Mode:                    UART0 / Hardware CDC
// Upload Speed:                   921600
// Programmer                      ESPTool

#include <Arduino.h>
#include <TFT_eSPI.h>    // please use the TFT_eSPI library found here: https://github.com/Xinyuan-LilyGO/T-Display-S3/tree/main/lib
#include "pin_config.h"  // found at https://github.com/Xinyuan-LilyGO/T-Display-S3/tree/main/example/factory
#include <NixieTubes.h>  //

// Define an instance of the NixieTubes class, call it myNixieTubes
NixieTubes myNixieTubes;

// create the TFT showPanel and sprite
TFT_eSPI showPanel = TFT_eSPI();
TFT_eSprite sprite = TFT_eSprite(&showPanel);

// Set the TFT display's dimensions here
int32_t TFT_Width = 320;
int32_t TFT_Height = 170;

// Misc variables
int tubeSize;
int xPosition;
int yPosition;
bool needToDrawTubes;
bool transparent;
String displayValue;

// Clears the sprite
void clearTheTFTDisplay(uint32_t backgroundColour) {
  sprite.fillSprite(backgroundColour);
}

// Updates the TFT display with what is in the sprite
void RefreshTheTFTDisplayWithItsTheNewImage() {
  sprite.pushSprite(0, 0);
}

// The following routine is used to display the example number as we progress through each example
void announceExample(int exampleNumber) {

  clearTheTFTDisplay(TFT_BLACK);

  // This next line will be explained further down in the sketch
  myNixieTubes.DrawNixieTubes(sprite, myNixieTubes.small, 0, 0, true, false, "Example:" + String(exampleNumber));
  RefreshTheTFTDisplayWithItsTheNewImage();
  delay(1000);
}

void pauseToViewTheResults() {
  delay(5000);  // pause for five seconds
}

void Introduction() {

  clearTheTFTDisplay(TFT_BLACK);

  // The following line of code will be explained in further detail in the sketch below
  myNixieTubes.DrawNixieTubes(sprite, myNixieTubes.small, 0, 0, true, false, "Nixie tube\n\rdemo");
  RefreshTheTFTDisplayWithItsTheNewImage();
  pauseToViewTheResults();
}

void Example1() {

  // Here we are using a size value available from the NixieTube Class
  //
  // Size values are:
  //    small,
  //    medium,
  //    large
  //
  // In pixels (width x height), these sizes are: small (32x75), medium (40x94) and large (64x151)
  // Accordingly, on a 320x170 TFT display in landscape this will allow a maximum of:
  //   small:   10 tubes per row, 2 rows per display
  //   medium:  8 tubes per row, 1 row per display
  //   medium:  5 tubes per row, 1 row per display

  // Draw a large size tube with the value of '1' at (0,0) of the display

  announceExample(1);

  tubeSize = myNixieTubes.large;
  xPosition = 0;
  yPosition = 0;
  needToDrawTubes = true;
  transparent = false;
  displayValue = "1";
  clearTheTFTDisplay(TFT_BLACK);
  myNixieTubes.DrawNixieTubes(sprite, tubeSize, xPosition, yPosition, needToDrawTubes, transparent, displayValue);
  RefreshTheTFTDisplayWithItsTheNewImage();
  pauseToViewTheResults();
}

void Example2() {

  // Draw a single medium sized tube with the value of '2' at (20, 25) of the display

  announceExample(2);

  tubeSize = myNixieTubes.medium;
  xPosition = 20;
  yPosition = 25;
  needToDrawTubes = true;
  transparent = false;
  displayValue = "2";
  clearTheTFTDisplay(TFT_BLACK);
  myNixieTubes.DrawNixieTubes(sprite, tubeSize, xPosition, yPosition, needToDrawTubes, transparent, displayValue);
  RefreshTheTFTDisplayWithItsTheNewImage();
  pauseToViewTheResults();
}

void Example3() {

  // Draw a single small tube with the value of '3' at postion in the centre of the display

  // Here we are using some dimensions values available from the NixieTube Class
  //
  // dimensions which are available are:
  //    tube Width,
  //    tube Hieght,
  //    the width of the screen inside the tube,
  //    the height of the screen inside the tube,
  //    the x offset of the screen inside the tube relative to top left of the tube,
  //    the y offset of the screen inside the tube relative to top left of the tube
  //
  // for more information see the file NixieTubes.h

  announceExample(3);

  tubeSize = myNixieTubes.small;
  xPosition = (TFT_Width / 2) - (myNixieTubes.dimensions[tubeSize][myNixieTubes.tubeWidth] / 2);
  yPosition = (TFT_Height / 2) - (myNixieTubes.dimensions[tubeSize][myNixieTubes.tubeHeight] / 2);
  needToDrawTubes = true;
  transparent = false;
  displayValue = "3";
  clearTheTFTDisplay(TFT_BLACK);
  myNixieTubes.DrawNixieTubes(sprite, tubeSize, xPosition, yPosition, needToDrawTubes, transparent, displayValue);
  RefreshTheTFTDisplayWithItsTheNewImage();
  pauseToViewTheResults();
}

void Example4() {

  // Show the large, medium and small tubes at varisous positions on the same display

  announceExample(4);

  needToDrawTubes = true;
  transparent = false;

  clearTheTFTDisplay(TFT_BLACK);

  tubeSize = myNixieTubes.large;
  xPosition = 0;
  yPosition = 0;
  displayValue = "L";
  myNixieTubes.DrawNixieTubes(sprite, tubeSize, xPosition, yPosition, needToDrawTubes, transparent, displayValue);

  tubeSize = myNixieTubes.medium;
  xPosition = 70;
  yPosition = 0;
  displayValue = "M";
  myNixieTubes.DrawNixieTubes(sprite, tubeSize, xPosition, yPosition, needToDrawTubes, transparent, displayValue);

  tubeSize = myNixieTubes.small;
  xPosition = 115;
  yPosition = 0;
  displayValue = "S";
  myNixieTubes.DrawNixieTubes(sprite, tubeSize, xPosition, yPosition, needToDrawTubes, transparent, displayValue);

  RefreshTheTFTDisplayWithItsTheNewImage();
  pauseToViewTheResults();
}

void Example5() {

  // Draw a series of large tubes starting at (0,0) of the display over the default (black) background

  announceExample(5);

  tubeSize = myNixieTubes.large;
  xPosition = 0;
  yPosition = 0;
  needToDrawTubes = true;
  transparent = false;
  displayValue = "12345";
  clearTheTFTDisplay(TFT_BLACK);
  myNixieTubes.DrawNixieTubes(sprite, tubeSize, xPosition, yPosition, needToDrawTubes, transparent, displayValue);
  RefreshTheTFTDisplayWithItsTheNewImage();
  pauseToViewTheResults();
}

void Example6() {

  // Set the background to white and draw a series of large tubes starting at (0,10) of the display
  // Note: the tubes appear over a black background, which in turn appears over the white background (likely not what you would want in most cases)

  announceExample(6);

  tubeSize = myNixieTubes.large;
  xPosition = 0;
  yPosition = 10;
  needToDrawTubes = true;
  transparent = false;
  displayValue = "67890";
  clearTheTFTDisplay(TFT_RED);
  myNixieTubes.DrawNixieTubes(sprite, tubeSize, xPosition, yPosition, needToDrawTubes, transparent, displayValue);
  RefreshTheTFTDisplayWithItsTheNewImage();
  pauseToViewTheResults();
}

void Example7() {

  // Set the background to white and draw a series of large tubes starting at (0,10) of the display + set the transparent flag to true
  // Note: with the transparent flag set to true the tubes blend in over the white background (more likley what you would want in most cases)

  announceExample(7);

  tubeSize = myNixieTubes.large;
  xPosition = 0;
  yPosition = 10;
  needToDrawTubes = true;
  transparent = true;
  displayValue = "67890";
  clearTheTFTDisplay(TFT_RED);
  myNixieTubes.DrawNixieTubes(sprite, tubeSize, xPosition, yPosition, needToDrawTubes, transparent, displayValue);
  RefreshTheTFTDisplayWithItsTheNewImage();
  pauseToViewTheResults();
}

void Example8() {

  // Display a series of small tubes over two lines, staring at (0,0) and (0,85)

  announceExample(8);

  tubeSize = myNixieTubes.small;
  xPosition = 0;
  yPosition = 0;
  needToDrawTubes = true;
  transparent = false;

  displayValue = "Hello";
  clearTheTFTDisplay(TFT_BLACK);
  myNixieTubes.DrawNixieTubes(sprite, tubeSize, xPosition, yPosition, needToDrawTubes, transparent, displayValue);

  yPosition = 85;
  displayValue = "World";
  myNixieTubes.DrawNixieTubes(sprite, tubeSize, xPosition, yPosition, needToDrawTubes, transparent, displayValue);
  RefreshTheTFTDisplayWithItsTheNewImage();
  pauseToViewTheResults();
}

void Example9() {

  // Display a series of small tubes over two lines using a carrage return (\r) and line feed (\n) in the display value to seperate the lines

  announceExample(9);

  tubeSize = myNixieTubes.small;
  xPosition = 0;
  yPosition = 0;
  needToDrawTubes = true;
  transparent = false;
  displayValue = "Hello\r\nWorld";

  clearTheTFTDisplay(TFT_BLACK);
  myNixieTubes.DrawNixieTubes(sprite, tubeSize, xPosition, yPosition, needToDrawTubes, transparent, displayValue);
  RefreshTheTFTDisplayWithItsTheNewImage();
  pauseToViewTheResults();
}

void Example10() {

  // Show all supported values; 20 at a time (10 on the top row, 10 on the bottom row)

  announceExample(10);

  tubeSize = myNixieTubes.small;
  xPosition = 0;
  yPosition = 0;
  needToDrawTubes = true;
  transparent = false;

  String ntsv = myNixieTubes.supportedValues;

  for (int i = 0; i <= myNixieTubes.numberOfSupportedValues - 1; i = i + 20) {

    displayValue = "";

    for (int j = i; j < i + 10; j++)
      displayValue.concat(ntsv.charAt(j));

    displayValue.concat("\r\n");

    for (int j = i + 10; j < i + 20; j++)
      displayValue.concat(ntsv.charAt(j));

    clearTheTFTDisplay(TFT_BLACK);
    myNixieTubes.DrawNixieTubes(sprite, tubeSize, xPosition, yPosition, needToDrawTubes, transparent, displayValue);
    RefreshTheTFTDisplayWithItsTheNewImage();
    pauseToViewTheResults();
  };
}

void Example11() {

  // For efficiency, once a tube is drawn if you want the tube to show another value within that same tube only the screen inside the tube needs to be redrawn (as opposed to the whole tube), this is done by setting the value of needToDrawTubes to false
  // This example will also show how quickly the tube values can be updated on the display

  // Draw a set of five blank tubes
  // Then progressively update the screen values of those tubes
  // note: medium will refresh marginally slower than small tubes, and large tubes will refresh marginally slower than medium tubes
  //       However, as a benchmark, five tubes (of any size) should be able to refresh over 30 times per second on an LilyGo T-Dispaly S3

  announceExample(11);

  tubeSize = myNixieTubes.small;
  xPosition = 0;
  yPosition = 0;
  needToDrawTubes = true;
  transparent = false;

  displayValue = "     ";
  clearTheTFTDisplay(TFT_BLACK);
  myNixieTubes.DrawNixieTubes(sprite, tubeSize, xPosition, yPosition, needToDrawTubes, transparent, displayValue);
  RefreshTheTFTDisplayWithItsTheNewImage();

  String ws;
  int i = 1;
  unsigned long tenSeconds = 10000;
  unsigned long endtime = millis() + tenSeconds;

  needToDrawTubes = false;

  while (millis() < endtime) {
    ws = "    " + String(i++);
    displayValue = ws.substring(ws.length() - 5);
    myNixieTubes.DrawNixieTubes(sprite, tubeSize, xPosition, yPosition, needToDrawTubes, transparent, displayValue);
    RefreshTheTFTDisplayWithItsTheNewImage();
  };

  needToDrawTubes = true;
  yPosition = 85;
  displayValue = "10 SECONDS";
  myNixieTubes.DrawNixieTubes(sprite, tubeSize, xPosition, yPosition, needToDrawTubes, transparent, displayValue);
  RefreshTheTFTDisplayWithItsTheNewImage();

  pauseToViewTheResults();
}

void Example12() {

  // If the display value contains a value for which there is no defined nixie tube it will be displayed as a blank tube

  // There are 95 supported values, these are: (space)!"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~
  // These can also be retrieved from myNixieTubes.supportedValues
  // For example:
  //   String mySupportedValues = myNixieTubes.supportedValues;

  announceExample(12);
  tubeSize = myNixieTubes.large;
  xPosition = 0;
  yPosition = 0;
  needToDrawTubes = true;
  transparent = true;
  displayValue = "AB" + String(char(7)) + "CD"; // note: chr(7) - the bell - is not supported
  clearTheTFTDisplay(TFT_BLACK);
  myNixieTubes.DrawNixieTubes(sprite, tubeSize, xPosition, yPosition, needToDrawTubes, transparent, displayValue);
  RefreshTheTFTDisplayWithItsTheNewImage();
  pauseToViewTheResults();
}

void Example13() {

  // if the display value runs off the end or bottom of the display, tubes that will not fully fit on the display will not be shown
  // the example below the display value is truncated after 10 tubes

  announceExample(13);

  tubeSize = myNixieTubes.small;
  xPosition = 0;
  yPosition = 0;
  needToDrawTubes = true;
  transparent = true;
  displayValue = "That is it, this part gets truncated as it runs off the right hand side of the screen\r\nThank you!\r\nthis part gets truncated too as it runs off the bottom of the screen";
  clearTheTFTDisplay(TFT_BLACK);
  myNixieTubes.DrawNixieTubes(sprite, tubeSize, xPosition, yPosition, needToDrawTubes, transparent, displayValue);
  RefreshTheTFTDisplayWithItsTheNewImage();
  pauseToViewTheResults();
}

void setup() {
 
  // This sketch assumes the use of a LilyGo T-Display S3

  // Turn on LilyGo T-Display S3's display
  pinMode(PIN_POWER_ON, OUTPUT);
  digitalWrite(PIN_POWER_ON, HIGH);

  // Initialize the show panel (consider this like the wooden frame over which a painter's canvas is attached)
  showPanel.init();
  showPanel.begin();

  // Set the orientation of the show panel
  showPanel.setRotation(1);  // 0 = 0 degrees , 1 = 90 degrees, 2 = 180 degrees, 3 = 270 degrees

  // Create a sprite (consider this like a canvas on which the image will be drawn)
  sprite.createSprite(TFT_Width, TFT_Height);
  sprite.setSwapBytes(true);

  // Initialize myNixieTubes
  myNixieTubes.setDisplayDimensions(TFT_Width, TFT_Height);
};

void loop() {

  Introduction();
 
  Example1();
  Example2();
  Example3();
  Example4();
  Example5();
  Example6();
  Example7();
  Example8();
  Example9();
  Example10();
  Example11();
  Example12();
  Example13();
}