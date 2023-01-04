// Copyright Rob Latour, 2023
// License: MIT
// https://raltour.com
// https://github.com/roblatour/NixieTubes

#ifndef NixieTubes_h
#define NixieTubes_h

#include <Arduino.h>
#include <TFT_eSPI.h>

class NixieTubes {

public:

  NixieTubes();
  void setDisplayDimensions(int32_t TFTWidth, int32_t TFTHeight);
  void getScreenDimensions(int tubeSize, int& Width, int& Height);
  void DrawNixieTubes(TFT_eSprite& sprite, int tubeSize, int xOffset, int yOffset, bool drawTheTubeItself, bool makeBackGroundTransparent, String tubeValues);

  static constexpr int numberOfSizes = 3;
  static constexpr int small = 0;
  static constexpr int medium = 1;
  static constexpr int large = 2;

  static constexpr int numberOfdimensions = 6;
  static constexpr int tubeWidth = 0;
  static constexpr int tubeHeight = 1;
  static constexpr int screenWidth = 2;
  static constexpr int screenHeight = 3;
  static constexpr int screenOffsetX = 4;
  static constexpr int screenOffsetY = 5;
  
  // the Nixie Tube background colour - used in drawing tubes with a transparent tube background over an existing background
  static constexpr int nixieTubeBackgroundColour = 0; // black
  
#include "supportedValuesAndDimensions.h"
 
private:

  uint16_t* getNixieTubeScreen(char charIn, int size);
  uint16_t* NixieTubeScreen = getNixieTubeScreen('0', large);

  // TFT Display width and height
  int32_t _TFTWidth;
  int32_t _TFTHeight;
 
  // before and after a nixie tube is drawn the original background is stored and then restored
  // the pixels of the original background kept in the array below
  // its dimensions are set to the width and height of the largest tube size
  // ideally this would be coded as:
  // uint16_t originalBackground[dimensions[large][tubeWidth]][dimensions[large][tubeHeight]];
  // however the compiler doesn't like that statement, so the values that would have otherwise been substituted are hardcoded below
  uint16_t originalBackground[64][151];
};

#endif
