// Copyright Rob Latour, 2023
// License: MIT
// https://raltour.com
// https://github.com/roblatour/NixieTubes

#include "NixieTubes.h"

NixieTubes::NixieTubes() {}

void NixieTubes::setDisplayDimensions(int32_t TFTWidth, int32_t TFTHeight) {
  _TFTWidth = TFTWidth;
  _TFTHeight = TFTHeight;
}

void NixieTubes::getScreenDimensions(int tubeSize, int& width, int& height) {
  // get the dimensions of the little screen inside the Nixie Tube

  width = NixieTubes::dimensions[tubeSize][NixieTubes::tubeWidth];
  height = NixieTubes::dimensions[tubeSize][NixieTubes::tubeHeight];
};

uint16_t* NixieTubes::getNixieTubeScreen(char charIn, int size) {

#include "NixieTubeScreens.h"

 uint16_t originalBackground[NixieTubes::dimensions[large][tubeWidth]][NixieTubes::dimensions[large][tubeHeight]];

 int index = supportedValues.indexOf(charIn);
  
  // if the character being requested to be drawn is not found, draw a blank screen (index = 0)
  if ((index < 0) || (index == numberOfSupportedValues))
	  index = 0;
  
  switch (size) {

      case small:
        {
          uint16_t* returnResult = new uint16_t[sizeof(nixieTubeSmallScreens[0])];
          memcpy(returnResult, nixieTubeSmallScreens[index], sizeof(nixieTubeSmallScreens[0]));
          return returnResult;
          break;
        }

      case medium:
        {
          uint16_t* returnResult = new uint16_t[sizeof(nixieTubeMediumScreens[0])];
          memcpy(returnResult, nixieTubeMediumScreens[index], sizeof(nixieTubeMediumScreens[0]));
          return returnResult;
		  break;
        }

      case large:
        {    
          uint16_t* returnResult = new uint16_t[sizeof(nixieTubeLargeScreens[0])];
          memcpy(returnResult, nixieTubeLargeScreens[index], sizeof(nixieTubeLargeScreens[0]));
          return returnResult;
		  break;    
         }
  };
}

void NixieTubes::DrawNixieTubes(TFT_eSprite& sprite, int tubeSize, int32_t xPos, int32_t yPos, bool drawTheTubeItself, bool makeBackGroundTransparent, String value) {

  int nixieTubeWidth = NixieTubes::dimensions[tubeSize][tubeWidth];
  int nixieTubeHeight = NixieTubes::dimensions[tubeSize][tubeHeight];
  int nixieTubeSreenWidth = NixieTubes::dimensions[tubeSize][screenWidth];
  int nixieTubeSreenHeight = NixieTubes::dimensions[tubeSize][screenHeight];
  int nixieTubeScreenOffsetX = NixieTubes::dimensions[tubeSize][screenOffsetX];
  int nixieTubeScreenOffsetY = NixieTubes::dimensions[tubeSize][screenOffsetY];

#include "nixieTubeTubes.h"

  int currentRow = 0;
  int currentColumn = 0;

  for (int i = 0; i < value.length(); i++) {
	  
	  // special carriage return and line feed handling
    if (value.charAt(i) == '\r')	  
		 currentColumn = 0;  // carriage return     
    else if (value.charAt(i) == '\n')
      currentRow++;  // line feed
    else {

      int32_t tubeXPos = xPos + (nixieTubeWidth * currentColumn);
      int32_t tubeYPos = yPos + (nixieTubeHeight * currentRow);

      // only draw the tubes/screens if they will fit fully on the TFT display
      if (!(((tubeXPos + nixieTubeWidth) > _TFTWidth) || ((tubeYPos + nixieTubeHeight) > _TFTHeight))) {

        if (drawTheTubeItself) {
			
          if (makeBackGroundTransparent)
            // save original background over which the Nixie Tube will be drawn
            for (int32_t x = 0; x < nixieTubeWidth; x++)
              for (int32_t y = 0; y < nixieTubeHeight; y++)
                originalBackground[x][y] = sprite.readPixel(tubeXPos + x, tubeYPos + y);

          // draw a blank Nixie Tube
          if (tubeSize == small)
            sprite.pushImage(tubeXPos, tubeYPos, nixieTubeWidth, nixieTubeHeight, nixieTubeSmall);
          else if (tubeSize == medium)
            sprite.pushImage(tubeXPos, tubeYPos, nixieTubeWidth, nixieTubeHeight, nixieTubeMedium);
          else
            sprite.pushImage(tubeXPos, tubeYPos, nixieTubeWidth, nixieTubeHeight, nixieTubeLarge);

          if (makeBackGroundTransparent)
            // restore original background under the transparent portions of the Nixie Tube
            for (int32_t x = 0; x < nixieTubeWidth; x++)
              for (int32_t y = 0; y < nixieTubeHeight; y++)
                if (sprite.readPixel(tubeXPos + x, tubeYPos + y) == nixieTubeBackgroundColour)
                  sprite.drawPixel(tubeXPos + x, tubeYPos + y, originalBackground[x][y]);
        };

        // draw the Nixie Tube screen associated with the character being worked on
        NixieTubeScreen = getNixieTubeScreen(value.charAt(i), tubeSize);

        if (sizeof(NixieTubeScreen) > 0)
          sprite.pushImage(tubeXPos + nixieTubeScreenOffsetX, tubeYPos + nixieTubeScreenOffsetY, nixieTubeSreenWidth, nixieTubeSreenHeight, NixieTubeScreen);

        delete[] NixieTubeScreen;

        currentColumn++;
      };
    };
  };
}
