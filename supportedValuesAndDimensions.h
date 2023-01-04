// Copyright Rob Latour, 2023
// License: MIT
// https://raltour.com
// https://github.com/roblatour/NixieTubes
//
// the number and values of supported Nixie tubes:
//
static const int numberOfSupportedValues = 95;

// the actual supported values:  
//
String supportedValues = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";

// In relation to the dimensions array below:                                                                                
//                                                                                                                           
//  the first dimension Is for the tube size, these are:                                                                     
//     small  (0),                                                                                                           
//     medium (1),                                                                                                           
//     large (2)                                                                                                             
//                                                                                                                           
//  the second dimension Is for the various dimensions associate with the tube And its the screen inside the tube, these are:
//    tube Width (0),                                                                                                        
//    tube Hieght (1),                                                                                                       
//    the width of the screen inside the tube (2),                                                                           
//    the height of the screen inside the tube (3),                                                                          
//    the x offset of the screen inside the tube relative to top left of the tube (4),                                       
//    the y offset of the screen inside the tube relative to top left of the tube (5)                                        
//                                                                                                                           
//  on a 320x170 TFT display in landscape this will allow a maximum of:                                                      
//    small   10 tubes per row, 2 rows per display                                                                           
//    medium:  8 tubes per row, 1 row per display                                                                            
//    medium:  5 tubes per row, 1 row per display                                                                            

int dimensions[numberOfSizes][numberOfdimensions] = {
    { 32, 71, 20, 29, 6, 22 },
    { 40, 94, 25, 39, 7, 29 },
    { 64, 151, 40, 62, 12, 47 }
  };

