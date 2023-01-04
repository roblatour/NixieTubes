' Copyright Rob Latour, 2023
' License: MIT
' https://raltour.com
' https://github.com/roblatour/NixieTubes

Imports System.Runtime.InteropServices
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports ImageMagick
Imports System.IO
Imports System.Text

Public Class Form1

    Private fontName As String = "Roboto" ' "Tuffy Bold" "Aharoni" "Nixie One"
    Private fontSize As Integer = 216

    Private largeTubeSize = New Size(64, 151)  '  5 tubes per row, 1 row per display
    Private mediumTubeSize = New Size(40, 94)  '  8 tubes per row, 1 row per display
    Private smallTubeSize = New Size(32, 71)   ' 10 tubes per row, 2 rows per display

    Private ArduinoNixieTubeLibraryName As String = "C:\Users\Rob Latour\Documents\Arduino\libraries\NixieTubes\"  ' Update this with the location of your Arduino Nixie Tube Library
    Private WorkingOutputDirectoryName As String = Path.GetTempPath() & "NixieTubes\"

    ' the target screen should be a square
    Private targetScreenWidth = 340
    Private targetScreenHeight = targetScreenWidth

    Private curvePercent As Single = 4
    Private widthReductionPercent As Integer = 12

    Private PrimaryLitTubeColour As Color = Color.FromArgb(255, 255, 0)  ' yellow

    Private TubeGlowColour As Color = Color.OrangeRed
    Private GlowBoarder As Integer = 6

    Private TubeBackgroundColour As Color = Color.FromArgb(0, 0, 0) ' black
    'Private TubeBackgroundColour As Color = Color.DarkOrange  
    'Private TubeBackgroundColour As Color = Color.FromArgb(255, 130, 0) ' black

    Private MeshColour As Color = Color.Silver
    Private MeshThickness As Integer = 1 ' 1 thin, 2 thick
    Private MeshSize As Integer = 20
    Private MeshSpacing As Integer = 2

    Private smallScreenSize As Size
    Private mediumScreenSize As Size
    Private largeScreenSize As Size

    Private smallScreenOffsetWithinTube As Point
    Private mediumScreenOffsetWithinTube As Point
    Private largeScreenOffsetWithinTube As Point

    Private OverallSupportedValues As String
    Private NumberOfSupportedValues As Integer

    Private workingScreenWidth As Integer = targetScreenWidth * (1.0 + 2 * (curvePercent / 100))
    Private workingScreenHeight As Integer = targetScreenHeight * (1.0 + 2 * (curvePercent / 100))
    Private workingScreenSize As Size = New Size(workingScreenWidth, workingScreenHeight)

    Private CreateAllScreeens As Boolean = True ' used for testing, if false only the character W will be processed

    Private Const TopOfHeaderComments As String = "// Copyright Rob Latour, 2023" & vbCrLf & "// License: MIT" & vbCrLf & "// https://raltour.com" & vbCrLf & "// https://github.com/roblatour/NixieTubes" & vbCrLf

#Region "Create Screens"

    ' screens are the little screens that fit inside the image of the Nixie Tube

    Private Function CreateACharacter(ByVal fontName As String, ByVal charCode As Integer, ByVal backgroundColour As Color, ByVal bitmapWidth As Integer, ByVal bitmapHeight As Integer, ByVal characterSize As Single) As Bitmap

        Dim bitmap As New Bitmap(bitmapWidth, bitmapHeight)

        Dim GP As New GraphicsPath

        Dim backgroundBrush = New SolidBrush(backgroundColour)

        Try

            Dim charToDraw As Char = Chr(charCode)

            Dim g As Graphics = Graphics.FromImage(bitmap)
            'Dim fnt As Font = New Font(fontName, characterSize, FontStyle.Regular)
            Dim fnt As Font = New Font(fontName, characterSize)
            Dim textFormat As New StringFormat()
            textFormat.Alignment = StringAlignment.Center
            textFormat.LineAlignment = StringAlignment.Center

            ' draw background
            g.FillRectangle(backgroundBrush, New Rectangle(0, 0, bitmapWidth, bitmapHeight))

            ' draw the character 
            fnt = New Font(fontName, characterSize)  '-3
            textFormat.Alignment = StringAlignment.Center
            textFormat.LineAlignment = StringAlignment.Center
            g.DrawString(charToDraw.ToString, fnt, New SolidBrush(PrimaryLitTubeColour), New RectangleF(0, 1, bitmapWidth, bitmapHeight), textFormat)

            ' Clean up
            fnt.Dispose()
            g.Dispose()

        Catch ex As Exception

        End Try

        Return bitmap

    End Function

    Private Sub applyGlow(ByRef bmp As Bitmap)

        If bmp Is Nothing Then Return

        Dim BitmapDimensions As Rectangle = New Rectangle(0, 0, bmp.Width, bmp.Height)

        Dim BitmapWidthMiunsOne As Integer = BitmapDimensions.Width - 1
        Dim BitmapHeightMiunsOne As Integer = BitmapDimensions.Height - 1

        Dim bmData As BitmapData
        Dim imgPixelFormat As PixelFormat = bmp.PixelFormat

        Dim imgPixelFormatString As String = imgPixelFormat.ToString

        Dim AdvanceBy As Integer

        If imgPixelFormatString.EndsWith("bppPArgb") Then
            AdvanceBy = 5
        ElseIf imgPixelFormatString.EndsWith("bppArgb") Then
            AdvanceBy = 4
        Else
            AdvanceBy = 3
        End If

        bmData = bmp.LockBits(BitmapDimensions, ImageLockMode.ReadWrite, imgPixelFormat)

        Dim bytes As Integer = bmData.Stride * bmp.Height
        Dim rgbValues As Byte() = New Byte(bytes - 1) {}
        Marshal.Copy(bmData.Scan0, rgbValues, 0, rgbValues.Length)

        Dim red As Double, green As Double, blue As Double
        Dim result As Color(,) = New Color(bmp.Width - 1, bmp.Height - 1) {}

        Dim rgb As Integer

        Dim imageX As Integer
        Dim imageY As Integer

        Dim xMax As Integer = Math.Min(BitmapDimensions.X + BitmapDimensions.Width, bmp.Width - 1)
        Dim yMax As Integer = Math.Min(BitmapDimensions.Y + BitmapDimensions.Height, bmp.Height - 1)

        Dim BitmapXPlusBitmapWidthMinus1 As Integer = BitmapDimensions.X + BitmapWidthMiunsOne
        Dim BitmapYPlusBitmapHeightMinus1 As Integer = BitmapDimensions.Y + BitmapHeightMiunsOne

        Dim effectsValue As Double

        Dim r, g, b As Integer

        Dim rgb_yValue As Integer

        Dim makeItGlow As Boolean

        Dim midRadius As Integer = Int(GlowBoarder / 2) + 1


        ' populate the result table x,y values with the original pixel color of the x, y pixels

        For x As Integer = BitmapDimensions.X To xMax - midRadius

            For y As Integer = BitmapDimensions.Y To yMax - midRadius

                Dim OriginalPoint As Integer = y * bmData.Stride + x * AdvanceBy

                red = CDbl(rgbValues(OriginalPoint + 2))
                green = CDbl(rgbValues(OriginalPoint + 1))
                blue = CDbl(rgbValues(OriginalPoint))

                result(x, y) = Color.FromArgb(red, green, blue)

            Next

        Next

        ' add a glow around the Lit Tube Colour at within the radius for points where the original colour is the background colour

        For x As Integer = BitmapDimensions.X To xMax

            For y As Integer = BitmapDimensions.Y To yMax

                Dim testColour As Color = Color.FromArgb(result(x, y).R, result(x, y).G, result(x, y).B)

                makeItGlow = (testColour = PrimaryLitTubeColour)

                If makeItGlow Then

                    For xGlow As Integer = -GlowBoarder To GlowBoarder

                        For yGlow As Integer = -GlowBoarder To GlowBoarder

                            Dim workingX = x + xGlow
                            Dim workingY = y + yGlow

                            If (workingX >= 0) AndAlso (workingY >= 0) AndAlso (workingX < xMax) AndAlso (workingY < yMax) Then

                                testColour = Color.FromArgb(result(workingX, workingY).R, result(workingX, workingY).G, result(workingX, workingY).B)

                                If testColour = TubeBackgroundColour Then

                                    '  Dim DistanceFromXY As Integer = (Math.Max(Math.Abs(xGlow), Math.Abs(yGlow))) * 6
                                    result(workingX, workingY) = Color.FromArgb(255, TubeGlowColour.R, TubeGlowColour.G, TubeGlowColour.B)

                                End If

                            End If

                        Next yGlow

                    Next xGlow

                End If

            Next y

        Next x

        For x As Integer = BitmapDimensions.X + 1 To BitmapDimensions.X + BitmapWidthMiunsOne

            For y As Integer = BitmapDimensions.Y + 1 To BitmapDimensions.Y + BitmapHeightMiunsOne

                rgb = y * bmData.Stride + x * AdvanceBy

                rgbValues(rgb + 3) = result(x, y).A
                rgbValues(rgb + 2) = result(x, y).R
                rgbValues(rgb + 1) = result(x, y).G
                rgbValues(rgb) = result(x, y).B

            Next

        Next

        System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, bmData.Scan0, rgbValues.Length)
        bmp.UnlockBits(bmData)


    End Sub

    Private Sub ApplyGaussianBlur(ByRef bmp As Bitmap)

        If bmp Is Nothing Then Return

        Dim BitmapDimensions As Rectangle = New Rectangle(0, 0, bmp.Width, bmp.Height)

        Dim BitmapWidthMiunsOne As Integer = BitmapDimensions.Width - 1
        Dim BitmapHeightMiunsOne As Integer = BitmapDimensions.Height - 1

        Dim bmData As BitmapData
        Dim imgPixelFormat As PixelFormat = bmp.PixelFormat

        Dim imgPixelFormatString As String = imgPixelFormat.ToString

        Dim AdvanceBy As Integer

        If imgPixelFormatString.EndsWith("bppPArgb") Then
            AdvanceBy = 5
        ElseIf imgPixelFormatString.EndsWith("bppArgb") Then
            AdvanceBy = 4
        Else
            AdvanceBy = 3
        End If

        bmData = bmp.LockBits(BitmapDimensions, ImageLockMode.ReadWrite, imgPixelFormat)

        Dim bytes As Integer = bmData.Stride * bmp.Height
        Dim rgbValues As Byte() = New Byte(bytes - 1) {}
        Marshal.Copy(bmData.Scan0, rgbValues, 0, rgbValues.Length)

        Dim red As Double, green As Double, blue As Double
        Dim result As Color(,) = New Color(bmp.Width - 1, bmp.Height - 1) {}

        Dim blurMatrix As Double(,) = {{3.7818, 3.9427, 3.99784, 3.9427, 3.7818},
                                       {3.9427, 4.11045, 4.16794, 4.11045, 3.9427},
                                       {3.99784, 4.16794, 4.22623, 4.16794, 3.99784},
                                       {3.9427, 4.11045, 4.16794, 4.11045, 3.9427},
                                       {3.7818, 3.9427, 3.99784, 3.9427, 3.7818}}
        Dim factor As Double = 0.01
        Dim bias As Double = 0

        'as an alternative to the above, the following is a less blurry blur

        'Dim BlurMatrix As Double(,) = {{0.00000066784770602513, 0.0000229160777551447, 0.000191169232389558, 0.00038771318114448, 0.000191169232389558, 0.0000229160777551447, 0.00000066784770602513},
        '{0.0000229160777551447, 0.000786326904984647, 0.0065596526787585, 0.0133037297660001, 0.0065596526787585, 0.000786326904984647, 0.0000229160777551447},
        '{0.000191169232389558, 0.0065596526787585, 0.0547215706256212, 0.110981636319217, 0.0547215706256212, 0.0065596526787585, 0.000191169232389558},
        '{0.00038771318114448, 0.0133037297660001, 0.110981636319217, 0.225083517510079, 0.110981636319217, 0.0133037297660001, 0.00038771318114448},
        '{0.000191169232389558, 0.0065596526787585, 0.0547215706256212, 0.110981636319217, 0.0547215706256212, 0.0065596526787585, 0.000191169232389558},
        '{0.0000229160777551447, 0.000786326904984647, 0.0065596526787585, 0.0133037297660001, 0.0065596526787585, 0.000786326904984647, 0.0000229160777551447},
        '{0.00000066784770602513, 0.0000229160777551447, 0.000191169232389558, 0.00038771318114448, 0.000191169232389558, 0.0000229160777551447, 0.00000066784770602513}}
        'Dim factor As Double = 0.84089642
        'Dim bias As Double = 0


        ' the Effects matrix = the Blur Matrix adjust for the factor and bias
        Dim effectsMatrixWidth As Integer = Math.Sqrt(blurMatrix.Length)
        Dim effectsMatrixHeight As Integer = effectsMatrixWidth

        Dim effectMatrix As Double(,) = New Double(effectsMatrixWidth, effectsMatrixHeight) {}

        For x = 0 To effectsMatrixWidth - 1
            For y = 0 To effectsMatrixHeight - 1
                effectMatrix(x, y) = blurMatrix(x, y)
                effectMatrix(x, y) += bias
                effectMatrix(x, y) *= factor
            Next
        Next

        Dim effectsMatrixOffset As Integer
        Select Case effectsMatrixWidth
            Case = 3
                effectsMatrixOffset = -1
            Case = 5
                effectsMatrixOffset = -2
            Case = 7
                effectsMatrixOffset = -3
        End Select

        Dim rgb As Integer

        Dim imageX As Integer
        Dim imageY As Integer

        Dim xMax As Integer = Math.Min(BitmapDimensions.X + BitmapDimensions.Width, bmp.Width - 1)
        Dim yMax As Integer = Math.Min(BitmapDimensions.Y + BitmapDimensions.Height, bmp.Height - 1)

        Dim BitmapXPlusBitmapWidthMinus1 As Integer = BitmapDimensions.X + BitmapWidthMiunsOne
        Dim BitmapYPlusBitmapHeightMinus1 As Integer = BitmapDimensions.Y + BitmapHeightMiunsOne

        Dim effectsMatrixHeightMinus1 As Integer = effectsMatrixHeight - 1
        Dim effectsMatrixWidthMinus1 As Integer = effectsMatrixWidth - 1

        Dim effectsValue As Double

        Dim r, g, b As Integer

        Dim rgb_yValue As Integer

        For y As Integer = BitmapDimensions.Y To yMax

            For x As Integer = BitmapDimensions.X To xMax

                red = 0
                green = 0
                blue = 0

                For effectsY As Integer = 0 To effectsMatrixHeightMinus1

                    imageY = y + effectsY + effectsMatrixOffset

                    If imageY > BitmapYPlusBitmapHeightMinus1 Then
                        imageY = BitmapYPlusBitmapHeightMinus1
                    End If

                    rgb_yValue = imageY * bmData.Stride

                    For effectsX As Integer = 0 To effectsMatrixWidthMinus1

                        imageX = x + effectsX + effectsMatrixOffset

                        If imageX > BitmapXPlusBitmapWidthMinus1 Then
                            imageX = BitmapXPlusBitmapWidthMinus1
                        End If

                        rgb = rgb_yValue + AdvanceBy * imageX

                        If rgb >= 0 Then

                            effectsValue = effectMatrix(effectsX, effectsY)

                            red += CDbl(rgbValues(rgb + 2)) * effectsValue
                            green += CDbl(rgbValues(rgb + 1)) * effectsValue
                            blue += CDbl(rgbValues(rgb)) * effectsValue

                        End If


                    Next effectsX

                    r = Math.Min(Math.Max(CInt(red), 0), 255)
                    g = Math.Min(Math.Max(CInt(green), 0), 255)
                    b = Math.Min(Math.Max(CInt(blue), 0), 255)

                    result(x, y) = Color.FromArgb(r, g, b)

                Next effectsY

            Next x

        Next y

        For x As Integer = BitmapDimensions.X + 1 To BitmapDimensions.X + BitmapWidthMiunsOne

            For y As Integer = BitmapDimensions.Y + 1 To BitmapDimensions.Y + BitmapHeightMiunsOne

                rgb = y * bmData.Stride + AdvanceBy * x

                rgbValues(rgb + 2) = result(x, y).R
                rgbValues(rgb + 1) = result(x, y).G
                rgbValues(rgb) = result(x, y).B

            Next

        Next

        System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, bmData.Scan0, rgbValues.Length)
        bmp.UnlockBits(bmData)

    End Sub

    Private Sub AddTheWiredBackground(ByRef bitmap As Bitmap)

        ' Set up the graphics object and pen
        Dim g As Graphics = Graphics.FromImage(bitmap)
        Dim pen As New Pen(MeshColour, MeshThickness)

        ' Set the size and spacing of the hexagons
        Dim hexSize As Integer = MeshSize '20
        Dim hexSpacing As Integer = MeshSpacing '2

        ' Determine the number of rows and columns that will fit on the graphics object
        Dim rows As Integer = g.VisibleClipBounds.Height / (hexSize + hexSpacing)
        Dim cols As Integer = g.VisibleClipBounds.Width / (hexSize + hexSpacing)

        ' Loop through the rows and columns, drawing hexagons at the appropriate locations

        Dim x As Integer
        Dim y As Integer
        Dim xOffset As Integer
        Dim yOffset As Integer = 0

        Dim offsetRequired As Boolean = False

        For row As Integer = 0 To rows * 2

            If offsetRequired Then
                xOffset = hexSize / 2
                yOffset += (hexSize / 2 - 2)
            Else
                xOffset = 0
                yOffset += hexSize / 4 + 2
            End If

            For col As Integer = 0 To cols * 2

                ' Calculate the center point of the hexagon

                x = col * (hexSize + hexSpacing - 1) + hexSize / 2 - xOffset
                y = row * (hexSize + hexSpacing) + hexSize / 2 - yOffset

                ' Create an array of points to hold the hexagon vertices
                Dim points(5) As Point

                ' Calculate the points for the hexagon
                points(0) = New Point(x, y - hexSize / 2)
                points(1) = New Point(x + hexSize / 2, y - hexSize / 4)
                points(2) = New Point(x + hexSize / 2, y + hexSize / 4)
                points(3) = New Point(x, y + hexSize / 2)
                points(4) = New Point(x - hexSize / 2, y + hexSize / 4)
                points(5) = New Point(x - hexSize / 2, y - hexSize / 4)

                ' Draw the hexagon
                g.DrawPolygon(pen, points)

            Next

            offsetRequired = Not offsetRequired

        Next

        ' Clean up
        pen.Dispose()
        g.Dispose()

    End Sub

    Private Function ApplyASlightLightingEffect(ByVal bm As Bitmap) As Bitmap

        ' apply a sligth lighting effect on the right

        ' ref: https://stackoverflow.com/questions/3232598/how-to-create-a-simple-glass-effect

        Dim backgroundColor As Color = Color.Snow
        Dim transparency As Integer = 0  ' 0 - 255

        Using g As Graphics = g.FromImage(bm)

            Dim brush As System.Drawing.Drawing2D.LinearGradientBrush

            g.DrawImage(bm, New Point(0, 0))

            g.InterpolationMode = InterpolationMode.HighQualityBicubic

            brush = New System.Drawing.Drawing2D.LinearGradientBrush(New Rectangle(0, 0, bm.Width * 3, bm.Height), Color.FromArgb(transparency, backgroundColor), backgroundColor, 0, True)
            g.FillRectangle(brush, New Rectangle(0, 0, bm.Width, bm.Height))

        End Using

        Return bm

    End Function

    Private Function WrapAroundATube(ByVal originalBitmap As Bitmap) As Bitmap

        ' Step 1
        ' Create a reference candidate bitmap with an updated size; tall enough to hold the finalized graphic
        ' also give it an all white background
        ' also save a curve line on the top of it which will be used as a reference towards creating the final result

        Dim CurveAmount As Single = originalBitmap.Height * (curvePercent / 100)

        Dim newHeight As Integer = originalBitmap.Height + CurveAmount

        Dim step1Bitmap As New Bitmap(originalBitmap.Width, newHeight)

        ' Create a graphics object based on the new bitmap
        Dim g As Graphics = Graphics.FromImage(step1Bitmap)

        ' fill the new bitmap with an all white background (which will also later be used as a test color)
        Dim TestColor As Color = Color.FromArgb(255, 255, 255, 255) ' all white

        Dim whiteBrush = New SolidBrush(TestColor)
        Dim newRect As Rectangle = New Rectangle(0, 0, step1Bitmap.Width, step1Bitmap.Height)

        g.FillRectangle(whiteBrush, newRect)

        'define the curve path
        Dim points As PointF() = {New PointF(0, 0), New PointF(step1Bitmap.Width / 2, CurveAmount), New PointF(step1Bitmap.Width, 0)}

        'draw the curve
        Dim tension As Single = 0.8
        g.DrawCurve(Pens.Black, points, tension)

        ' Step 2
        ' Save the new candiate referance bitmap, not sure why saving it is required, but it is

        Dim step2Bitmap = New Bitmap(step1Bitmap)


        ' Step 3 
        ' using the original bitmap and referance bitmap
        ' create a near final bitmap 
        ' and then map each of the pixels in the original bitmap to the near final bitmap 
        ' such that each pixel remains in its original x position, but its y position is offset by the height of where the curve begins on the refences bitmap

        Dim step3Bitmap As New Bitmap(step2Bitmap)

        Dim newStartingHeight As Integer = 0

        For x As Integer = 0 To originalBitmap.Width - 1

            For y As Integer = 0 To originalBitmap.Height - 1

                If step2Bitmap.GetPixel(x, y) <> TestColor Then
                    newStartingHeight = y
                    Exit For
                End If

            Next

            For y As Integer = 0 To originalBitmap.Height - 1
                step3Bitmap.SetPixel(x, y + newStartingHeight, originalBitmap.GetPixel(x, y))
            Next

        Next


        ' Step 4
        ' to create the final image
        ' now that the image has been curved and its height adjusted, its width needs to be reduced

        ' Calculate the new width of the image
        Dim WidthAdjustment As Integer = originalBitmap.Width * (1.0 - curvePercent / 100)
        Dim newWidth As Integer = WidthAdjustment

        ' Create a new bitmap with the new width
        Dim step4Bitmap As New Bitmap(newWidth, newHeight)

        ' Create a graphics object to perform the resizing
        Using g2 As Graphics = Graphics.FromImage(step4Bitmap)
            ' Set the interpolation mode to high quality to ensure a good result
            g2.InterpolationMode = InterpolationMode.HighQualityBicubic

            ' Draw the resized image
            g2.DrawImage(step3Bitmap, 0, 0, newWidth, step3Bitmap.Height)
        End Using


        ' Step 5
        ' crop the image

        newHeight -= CurveAmount * 2 - 2

        Dim cropRect = New Rectangle(0, CurveAmount, newWidth, newHeight)

        Dim finalScreenWidth As Integer = targetScreenWidth ' originalBitmap.Width * ((1.0 - curvePercent / 100))
        Dim finalScreenHeight As Integer = finalScreenWidth
        Dim step5Bitmap = New Bitmap(finalScreenWidth, finalScreenHeight)

        Using g3 = Graphics.FromImage(step5Bitmap)
            g3.DrawImage(step4Bitmap, New Rectangle(0, 0, cropRect.Width, cropRect.Height), cropRect, GraphicsUnit.Pixel)
        End Using


        'clean up and return
        step1Bitmap.Dispose()
        step2Bitmap.Dispose()
        step3Bitmap.Dispose()
        step4Bitmap.Dispose()

        Return step5Bitmap

    End Function

    Private Sub resizeBitmap(ByRef bm As Bitmap, ByVal newSize As Size)

        ' bm = New Bitmap(bm, newSize)  -  this works but does not look good, so below MagicK is used for better results

        Dim quality As Integer = 100

        Dim magicFactory = New MagickFactory()
        Dim magicImage As MagickImage = New MagickImage(magicFactory.Image.Create(bm))

        magicImage.Strip()
        magicImage.Quality = quality

        Dim geometry As MagickGeometry = New MagickGeometry(newSize.Width, newSize.Height)
        geometry.IgnoreAspectRatio = True

        magicImage.Resize(geometry)

        Using Memory As New MemoryStream

            magicImage.Write(Memory)

            bm = New System.Drawing.Bitmap(Memory)

        End Using

    End Sub

    Private Function FitScreenInTube(ByVal screen As Bitmap, ByVal TubeSize As Size) As Bitmap

        Dim tube As New Bitmap(My.Resources.tube)

        Dim centerX As Integer = tube.Width / 2
        Dim centerY As Integer = tube.Height / 2

        Dim centerColour As Color = tube.GetPixel(centerX, centerY)

        ' Find the top left x, y postion within the tube add the screen

        Dim TopLeftX, TopLeftY

        For x = centerX To 0 Step -1

            If tube.GetPixel(x, centerY) <> centerColour Then
                TopLeftX = x + 1
                Exit For
            End If

        Next

        For y = centerY To 0 Step -1

            If tube.GetPixel(centerX, y) <> centerColour Then
                TopLeftY = y + 1
                Exit For
            End If

        Next

        ' Find the bottom right x, y postion within the tube add the screen

        Dim BottomRightX, BottomRightY

        For x = centerX To tube.Width - 1

            If tube.GetPixel(x, centerY) <> centerColour Then
                BottomRightX = x - 1
                Exit For
            End If

        Next

        For y = centerY To tube.Height - 1

            If tube.GetPixel(centerX, y) <> centerColour Then
                BottomRightY = y - 1
                Exit For
            End If

        Next

        Dim TubeScreenWidth As Integer = BottomRightX - TopLeftX
        Dim TubeScreenHeight As Integer = BottomRightY - TopLeftY

        ' resize the screen to fit in the original full sized tube
        Dim newscreen = New Bitmap(screen)
        Dim newScreenSize = New Drawing.Size(TubeScreenWidth, TubeScreenHeight)
        resizeBitmap(newscreen, newScreenSize)

        ' place the screen into the original full-sized tube
        Dim srcRectangle As New Rectangle(0, 0, screen.Width, screen.Height)
        Dim destRectangle As New Rectangle(TopLeftX, TopLeftY, newscreen.Width, newscreen.Height)

        Using g As Graphics = Graphics.FromImage(tube)
            g.DrawImage(newscreen, destRectangle, srcRectangle, GraphicsUnit.Pixel)
        End Using

        'check point this work
        Dim newTubeAndScreen = New Bitmap(tube)

        'resize the full sized tube with screen to the requested (small, medium or large) size
        resizeBitmap(newTubeAndScreen, TubeSize)

        'calculate the screen's offset and size within the newly created tube
        'these will be stored globally now, but used later in the program elsewhere

        Dim FinalRelativeWidthOfScreenWithinTube As Single = newscreen.Width / tube.Width
        Dim FinalRelativeHeightOfScreenWithinTube As Single = newscreen.Height / tube.Height

        Dim RevisedScreenSize As New Size(TubeSize.Width * FinalRelativeWidthOfScreenWithinTube, TubeSize.Height * FinalRelativeHeightOfScreenWithinTube)
        TopLeftX *= (newTubeAndScreen.Width / tube.Width)
        TopLeftY *= (newTubeAndScreen.Height / tube.Height)

        If TubeSize = largeTubeSize Then
            largeScreenSize = RevisedScreenSize
            largeScreenOffsetWithinTube = New Point(TopLeftX, TopLeftY)
        Else
            If TubeSize = mediumTubeSize Then
                mediumScreenSize = RevisedScreenSize
                mediumScreenOffsetWithinTube = New Point(TopLeftX, TopLeftY)
            Else
                smallScreenSize = RevisedScreenSize
                smallScreenOffsetWithinTube = New Point(TopLeftX, TopLeftY)
            End If
        End If

        Return newTubeAndScreen

    End Function

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load

        Me.PictureBox1.Size = workingScreenSize
        Me.PictureBox2.Size = workingScreenSize
        Me.PictureBox3.Size = workingScreenSize
        Me.PictureBox4.Size = workingScreenSize
        Me.PictureBox5.Size = workingScreenSize
        Me.PictureBox6.Size = workingScreenSize
        Me.PictureBox7.Size = workingScreenSize
        Me.PictureBox8.Size = workingScreenSize
        Me.PictureBox9.Size = workingScreenSize
        Me.PictureBox10.Size = workingScreenSize

        Dim spacer = 25
        PictureBox1.Location = New Point(spacer, 10)
        PictureBox2.Location = New Point(workingScreenSize.Width + spacer * 2, 10)
        PictureBox3.Location = New Point(workingScreenSize.Width * 2 + spacer * 3, 10)
        PictureBox4.Location = New Point(workingScreenSize.Width * 3 + spacer * 4, 10)

        PictureBox5.Location = New Point(spacer, 390)
        PictureBox6.Location = New Point(workingScreenSize.Width + spacer * 2, 390)
        PictureBox7.Location = New Point(workingScreenSize.Width * 2 + spacer * 3, 390)

        Dim tube As New Bitmap(My.Resources.tube)
        Me.PictureBox11.Image = tube
        Me.PictureBox11.Width = largeTubeSize.width
        Me.PictureBox11.Height = largeTubeSize.height
        Me.PictureBox11.SizeMode = PictureBoxSizeMode.StretchImage

        PictureBox11.Location = New Point(PictureBox7.Location.X + 400, 390) ' blank tube

        PictureBox8.Location = New Point(PictureBox11.Location.X + 80, 390) ' small
        PictureBox9.Location = New Point(PictureBox11.Location.X + 160, 390) ' medium
        PictureBox10.Location = New Point(PictureBox11.Location.X + 240, 390) ' large

        Me.Width = workingScreenSize.Width * 4 + spacer * 5 + 10

        Me.Refresh()

    End Sub

    Private Sub CreateAllScreens()

        Dim ws As String

        Dim FileName As String

        Dim bm As Bitmap

        Dim AScreenImage1 As Bitmap = Nothing
        Dim AScreenImage2 As Bitmap = Nothing
        Dim AScreenImage3 As Bitmap = Nothing
        Dim AScreenImage4 As Bitmap = Nothing
        Dim AScreenImage5 As Bitmap = Nothing
        Dim AScreenImage6 As Bitmap = Nothing
        Dim AScreenImage7 As Bitmap = Nothing
        Dim AScreenImage8 As Bitmap = Nothing
        Dim AScreenImage9 As Bitmap = Nothing
        Dim AScreenImage10 As Bitmap = Nothing

        Dim firstTube As Integer
        Dim lastTube As Integer

        Dim CharW As Integer = 87 ' 87 is the ascii code for the capital letter 'W'; this will be the 'test' tube draw if the user doesn't select to generate them all

        If CreateAllScreeens Then
            'Create all Screens: 32 .. 126
            firstTube = 32
            lastTube = 126
        Else
            ' create only the screen for the Capital W
            firstTube = CharW
            lastTube = CharW
        End If

        Dim resultToSave As Bitmap

        If System.IO.Directory.Exists(WorkingOutputDirectoryName) Then
        Else
            System.IO.Directory.CreateDirectory(WorkingOutputDirectoryName)
        End If

        For Each ExistingFileName As String In Directory.GetFiles(WorkingOutputDirectoryName)
            File.Delete(ExistingFileName)
        Next

        For i As Integer = firstTube To lastTube

            Dim screenasciicode As String = i.ToString.PadLeft(3, "0")

            FileName = WorkingOutputDirectoryName & "NixieTube_screen_original_WidthxHeight_" & screenasciicode & ".png"

            ' create the character associated with ascii code i
            bm = CreateACharacter(fontName, i, TubeBackgroundColour, workingScreenWidth, workingScreenHeight, fontSize)
            AScreenImage1 = New Bitmap(bm)

            ' apply a glow to the character
            applyGlow(bm)
            AScreenImage2 = New Bitmap(bm)

            'blur the character
            ApplyGaussianBlur(bm)
            AScreenImage3 = New Bitmap(bm)

            'apply a faint light shading effect
            bm = ApplyASlightLightingEffect(bm)
            AScreenImage4 = New Bitmap(bm)

            'add wires in the form of a honeycomb mesh
            AddTheWiredBackground(bm)
            AScreenImage5 = New Bitmap(bm)

            'warp the 2d image slightly, as if it was be place around a tube
            bm = WrapAroundATube(bm)
            AScreenImage6 = New Bitmap(bm)

            'narrow the image to simulate it being wrapped in a tube
            Dim bmw As Integer = bm.Width * (100 - widthReductionPercent) / 100
            Dim bmh As Integer = bm.Height
            resizeBitmap(bm, New Size(bmw, bmh))
            AScreenImage7 = New Bitmap(bm)

            ' save the original finalized full sized screen

            ws = FileName.Replace("Width", AScreenImage7.Width).Replace("Height", AScreenImage7.Height)
            AScreenImage7.Save(ws, ImageFormat.Png)

            Dim fullSizedScreen = New Bitmap(bm)

            ' resize the full-sized tube and full-sized screen to small, medium and large
            ' in this process also calculate the the screen offset from the top left of the image, as well as its size
            ' finally save the screen to disk


            ' create small screens
            Me.PictureBox8.Image = FitScreenInTube(fullSizedScreen, smallTubeSize)

            Dim small As New Bitmap(fullSizedScreen)
            resizeBitmap(small, smallScreenSize)

            ws = FileName.Replace("original", "small").Replace("Width", small.Width).Replace("Height", small.Height)
            resultToSave = New Bitmap(small)
            resultToSave.Save(ws, ImageFormat.Png)


            ' create medium screens
            Me.PictureBox9.Image = FitScreenInTube(fullSizedScreen, mediumTubeSize)

            Dim medium As New Bitmap(fullSizedScreen)
            resizeBitmap(medium, mediumScreenSize)

            ws = FileName.Replace("original", "medium").Replace("Width", medium.Width).Replace("Height", medium.Height)
            resultToSave = New Bitmap(medium)
            resultToSave.Save(ws, ImageFormat.Png)


            ' create large screens
            Me.PictureBox10.Image = FitScreenInTube(fullSizedScreen, largeTubeSize)

            Dim large As New Bitmap(fullSizedScreen)
            resizeBitmap(large, largeScreenSize)

            ws = FileName.Replace("original", "large").Replace("Width", large.Width).Replace("Height", large.Height)
            resultToSave = New Bitmap(large)
            resultToSave.Save(ws, ImageFormat.Png)


            ' update the display 

            Me.PictureBox1.Image = New Bitmap(AScreenImage1)
            Me.PictureBox2.Image = New Bitmap(AScreenImage2)
            Me.PictureBox3.Image = New Bitmap(AScreenImage3)
            Me.PictureBox4.Image = New Bitmap(AScreenImage4)
            Me.PictureBox5.Image = New Bitmap(AScreenImage5)
            Me.PictureBox6.Image = New Bitmap(AScreenImage6)
            Me.PictureBox7.Image = New Bitmap(AScreenImage7)
            Me.PictureBox7.Size = New Size(AScreenImage7.Width, AScreenImage7.Height)

            Me.Refresh()

            Application.DoEvents()

        Next

        ' Now draw and save the original, large, medium and small sized tubes 

        Dim tube As Bitmap
        Dim OriginalFileName = WorkingOutputDirectoryName & "NixieTube_tube_Original_XxY.png"

        ' original 

        tube = New Bitmap(My.Resources.tube)
        resultToSave = New Bitmap(tube)
        FileName = OriginalFileName
        FileName = FileName.Replace("Xx", tube.Width & "x").Replace("xY", "x" & tube.Height)
        resultToSave.Save(FileName, ImageFormat.Png)

        ' small 

        tube = New Bitmap(My.Resources.tube)
        resizeBitmap(tube, largeTubeSize)
        FileName = OriginalFileName.Replace("Original", "Large")
        FileName = FileName.Replace("Xx", tube.Width & "x").Replace("xY", "x" & tube.Height)
        resultToSave = New Bitmap(tube)
        resultToSave.Save(FileName, ImageFormat.Png)

        ' medium 

        tube = New Bitmap(My.Resources.tube)
        resizeBitmap(tube, mediumTubeSize)
        FileName = OriginalFileName.Replace("Original", "Medium")
        FileName = FileName.Replace("Xx", tube.Width & "x").Replace("xY", "x" & tube.Height)
        resultToSave = New Bitmap(tube)
        resultToSave.Save(FileName, ImageFormat.Png)

        ' large 

        tube = New Bitmap(My.Resources.tube)
        resizeBitmap(tube, smallTubeSize)
        FileName = OriginalFileName.Replace("Original", "Small")
        FileName = FileName.Replace("Xx", tube.Width & "x").Replace("xY", "x" & tube.Height)
        resultToSave = New Bitmap(tube)
        resultToSave.Save(FileName, ImageFormat.Png)

    End Sub

#End Region

#Region "Create Header Files"
    Private Function ProperCase(ByVal s As String) As String

        Return s.Substring(0, 1).ToUpper & s.Substring(1)

    End Function

    Public Function Rgb888ToRgb565(ByVal wc As Color) As UInt16

        Dim red As Byte = wc.R
        Dim green As Byte = wc.G
        Dim blue As Byte = wc.B

        Dim red565 As UInt16 = CShort(red >> 3)
        Dim green565 As UInt16 = CShort(green >> 2)
        Dim blue565 As UInt16 = CShort(blue >> 3)

        Return CInt((red565 << 11) Or (green565 << 5) Or blue565)

    End Function

    Private Sub WriteHeaderFileForASpecifiedSizedScreen(ByVal AllFiles As FileInfo())

        ' example file.name : NixieTube_screen_large_48x52_087.png

        Static Dim SupportedValues As New StringBuilder

        Static Dim HeaderFile As New StringBuilder

        Dim FileNameParts() As String = IO.Path.GetFileNameWithoutExtension(AllFiles(0).Name).Split("_")
        Dim FileNameSize As String = FileNameParts(2) '   large, medium, small
        Dim FileNameDimensions As String = FileNameParts(3)  ' WidthxHeight, for example: 48x52
        Dim FileNameCharacter As String = FileNameParts(4)   ' this is the ascii code for the character, for example 87 for the capital letter 'W'
        Dim FileNameCharacterValue As Integer = FileNameCharacter

        Dim FileNameWidthAndHeight = FileNameDimensions.Split("x")
        Dim Width As Integer = FileNameWidthAndHeight(0)
        Dim Height As Integer = FileNameWidthAndHeight(1)

        Dim MatrixSize As Integer = Width * Height

        SupportedValues.Clear()
        ' SupportedValues.Append("String nixieTube" & ProperCase(FileNameSize) & "ScreenSupportedValues = """)
        SupportedValues.Append("String supportedValues = """)

        ' Begin with the following (as an example):
        '
        ' //   nixie tube small screens (26x28):
        '
        ' const static uint16_t nixieTubeSmallScreens[95][728] = {
        '
        HeaderFile.Clear()
        HeaderFile.Append("//   nixie tube " & FileNameSize & " screens (" & FileNameDimensions & "):" & vbCrLf)
        HeaderFile.Append(vbCrLf)
        HeaderFile.Append("const static uint16_t nixieTube" & ProperCase(FileNameSize) & "Screens[" & AllFiles.Count & "][" & MatrixSize.ToString.Trim & "] = {" & vbCrLf)
        HeaderFile.Append(vbCrLf)

        For Each file In AllFiles

            FileNameParts = file.Name.Split("_")

            FileNameSize = FileNameParts(2) ' large, medium, small
            FileNameDimensions = FileNameParts(3)  ' WidthxHeight, for example: 48x52
            FileNameCharacter = FileNameParts(4).Replace(".png", "")   ' this is the ascii code for the character, for example 87 for the capital letter 'W'
            FileNameCharacterValue = FileNameCharacter

            FileNameWidthAndHeight = FileNameDimensions.Split("x")
            Width = FileNameWidthAndHeight(0)
            Height = FileNameWidthAndHeight(1)

            Dim bm As New Bitmap(file.FullName)

            'double check the filename matches the file's contents
            If (Width <> bm.Width) OrElse (Height <> bm.Height) Then
                Beep()
                MsgBox("something is amiss")
            End If

            ' add the following 

            '  // ascii character
            '  { 

            HeaderFile.Append("  // ascii character    '" & Chr(FileNameCharacterValue) & "'" & vbCrLf)
            HeaderFile.Append("  // ascii code value = " & FileNameCharacter & vbCrLf)
            HeaderFile.Append("  { ")

            If (FileNameCharacterValue = 34) OrElse (FileNameCharacterValue = 92) Then
                ' special case for the quote and the backslash; both needs to be proceeded by a backslash
                SupportedValues.Append("\")
            End If
            SupportedValues.Append(Chr(FileNameCharacterValue))

            '' add the table entries

            Dim hexValue As String
            Dim rgb565Value As UInt16

            For h = 0 To Height - 1

                For w = 0 To Width - 1

                    rgb565Value = Rgb888ToRgb565(bm.GetPixel(w, h))
                    hexValue = "0x" & rgb565Value.ToString("X4")

                    If (w = Width - 1) AndAlso (h = Height - 1) Then
                        HeaderFile.Append(hexValue & " }, ")
                    Else
                        HeaderFile.Append(hexValue & ", ")
                    End If

                Next

                HeaderFile.Append(vbCrLf & "    ")

            Next

            HeaderFile.Append(vbCrLf)

        Next

        SupportedValues.Append(""";")

        Dim pos As Integer = HeaderFile.ToString.LastIndexOf(",")

        HeaderFile.Remove(pos, HeaderFile.Length - pos)

        HeaderFile.Append(vbCrLf & "};")

        Dim HeaderFileName As String = "nixieTube" & ProperCase(FileNameSize) & "Screens.h"

        File.WriteAllText(WorkingOutputDirectoryName & HeaderFileName, vbCrLf & HeaderFile.ToString)

        NumberOfSupportedValues = AllFiles.Count
        OverallSupportedValues = SupportedValues.ToString & vbCrLf

    End Sub
    Private Sub WriteHeaderFileForASpecifiedSizedTube(ByVal AllFiles As FileInfo())

        ' example file.name : NixieTube_tube_Large_64x151.png

        Static Dim HeaderFile As New StringBuilder

        Dim FileNameParts() As String = IO.Path.GetFileNameWithoutExtension(AllFiles(0).Name).Split("_")
        Dim FileNameSize As String = FileNameParts(2) '   large, medium, small
        Dim FileNameDimensions As String = FileNameParts(3)  ' WidthxHeight, for example: 48x52

        Dim FileNameWidthAndHeight = FileNameDimensions.Split("x")
        Dim Width As Integer = FileNameWidthAndHeight(0)
        Dim Height As Integer = FileNameWidthAndHeight(1)

        Dim MatrixSize As Integer = Width * Height

        HeaderFile.Clear()
        HeaderFile.Append("//   " & FileNameSize & " Nixie Tube (" & FileNameDimensions & ")" & vbCrLf)

        Dim screenOffset As String
        Dim screenSize As String


        If FileNameSize = "Large" Then
            screenSize = largeScreenSize.Width & "x" & largeScreenSize.Height
            screenOffset = largeScreenOffsetWithinTube.X & "x" & largeScreenOffsetWithinTube.Y
        Else
            If FileNameSize = "Medium" Then
                screenSize = mediumScreenSize.Width & "x" & mediumScreenSize.Height
                screenOffset = mediumScreenOffsetWithinTube.X & "x" & mediumScreenOffsetWithinTube.Y
            Else
                screenSize = smallScreenSize.Width & "x" & smallScreenSize.Height
                screenOffset = smallScreenOffsetWithinTube.X & "x" & smallScreenOffsetWithinTube.Y
            End If
        End If

        HeaderFile.Append("//   the size of the screen within the tube is " & screenOffset & vbCrLf)
        HeaderFile.Append("//   the offset of the screen within the tube is " & screenSize & vbCrLf)

        HeaderFile.Append(vbCrLf)
        HeaderFile.Append("const static uint16_t nixieTube" & ProperCase(FileNameSize) & "[" & MatrixSize.ToString.Trim & "] = {" & vbCrLf & "    ")

        For Each file In AllFiles

            Dim bm As New Bitmap(file.FullName)

            'double check the filename matches the file's contents
            If (Width <> bm.Width) OrElse (Height <> bm.Height) Then
                Beep()
                MsgBox("something is amiss")
            End If

            '' add the table entries

            Dim hexValue As String
            Dim rgb565Value As UInt16

            For h = 0 To Height - 1

                For w = 0 To Width - 1

                    rgb565Value = Rgb888ToRgb565(bm.GetPixel(w, h))
                    hexValue = "0x" & rgb565Value.ToString("X4")

                    If (w = Width - 1) AndAlso (h = Height - 1) Then
                        HeaderFile.Append(hexValue & " };")
                    Else
                        HeaderFile.Append(hexValue & ", ")
                    End If

                Next

                HeaderFile.Append(vbCrLf & "    ")

            Next

            HeaderFile.Append(vbCrLf)

        Next

        HeaderFile.Append(vbCrLf)

        Dim HeaderFileName As String = "nixieTube" & ProperCase(FileNameSize) & ".h"

        File.WriteAllText(WorkingOutputDirectoryName & HeaderFileName, HeaderFile.ToString)


    End Sub

    Private Sub CreateAllHeaderFiles()

        ' create the the core data for the header files 

        Dim di As New DirectoryInfo(WorkingOutputDirectoryName)

        WriteHeaderFileForASpecifiedSizedScreen(di.GetFiles("*screen_small*.png"))
        WriteHeaderFileForASpecifiedSizedScreen(di.GetFiles("*screen_medium*.png"))
        WriteHeaderFileForASpecifiedSizedScreen(di.GetFiles("*screen_large*.png"))

        WriteHeaderFileForASpecifiedSizedTube(di.GetFiles("*tube_small*.png"))
        WriteHeaderFileForASpecifiedSizedTube(di.GetFiles("*tube_medium*.png"))
        WriteHeaderFileForASpecifiedSizedTube(di.GetFiles("*tube_large*.png"))

        Dim ws As String

        ' create the tubes header file
        ws = TopOfHeaderComments
        ws &= IO.File.ReadAllText(WorkingOutputDirectoryName & "nixieTubeSmall.h")
        ws &= IO.File.ReadAllText(WorkingOutputDirectoryName & "nixieTubeMedium.h")
        ws &= IO.File.ReadAllText(WorkingOutputDirectoryName & "nixieTubeLarge.h")

        If File.Exists(ArduinoNixieTubeLibraryName & "supportedValuesAndDimensions.h") Then
            File.Delete(ArduinoNixieTubeLibraryName & "nixieTubeTubes.h")
        End If

        IO.File.WriteAllText(ArduinoNixieTubeLibraryName & "nixieTubeTubes.h", ws)

        IO.File.Delete(WorkingOutputDirectoryName & "nixieTubeSmall.h")
        IO.File.Delete(WorkingOutputDirectoryName & "nixieTubeMedium.h")
        IO.File.Delete(WorkingOutputDirectoryName & "nixieTubeLarge.h")

        ' create the screens header file

        ws = TopOfHeaderComments
        ws &= IO.File.ReadAllText(WorkingOutputDirectoryName & "nixieTubeSmallScreens.h")
        ws &= IO.File.ReadAllText(WorkingOutputDirectoryName & "nixieTubeMediumScreens.h")
        ws &= IO.File.ReadAllText(WorkingOutputDirectoryName & "nixieTubeLargeScreens.h")

        If File.Exists(ArduinoNixieTubeLibraryName & "supportedValuesAndDimensions.h") Then
            File.Delete(ArduinoNixieTubeLibraryName & "nixieTubeScreens.h")
        End If

        IO.File.WriteAllText(ArduinoNixieTubeLibraryName & "nixieTubeScreens.h", ws)

        IO.File.Delete(WorkingOutputDirectoryName & "nixieTubeSmallScreens.h")
        IO.File.Delete(WorkingOutputDirectoryName & "nixieTubeMediumScreens.h")
        IO.File.Delete(WorkingOutputDirectoryName & "nixieTubeLargeScreens.h")

        ' create supported values and dimesions header file

        ws = TopOfHeaderComments
        ws &= "//" & vbCrLf
        ws &= "// the number and values of supported Nixie tubes:" & vbCrLf
        ws &= "//" & vbCrLf
        ws &= "static const int numberOfSupportedValues = " & NumberOfSupportedValues & ";" & vbCrLf
        ws &= vbCrLf
        ws &= "// the actual supported values:  " & vbCrLf
        ws &= "//" & vbCrLf
        ws &= OverallSupportedValues
        ws &= vbCrLf
        ws &= "// In relation to the dimensions array below:                                                                                " & vbCrLf
        ws &= "//                                                                                                                           " & vbCrLf
        ws &= "//  the first dimension Is for the tube size, these are:                                                                     " & vbCrLf
        ws &= "//     small  (0),                                                                                                           " & vbCrLf
        ws &= "//     medium (1),                                                                                                           " & vbCrLf
        ws &= "//     large (2)                                                                                                             " & vbCrLf
        ws &= "//                                                                                                                           " & vbCrLf
        ws &= "//  the second dimension Is for the various dimensions associate with the tube And its the screen inside the tube, these are:" & vbCrLf
        ws &= "//    tube Width (0),                                                                                                        " & vbCrLf
        ws &= "//    tube Hieght (1),                                                                                                       " & vbCrLf
        ws &= "//    the width of the screen inside the tube (2),                                                                           " & vbCrLf
        ws &= "//    the height of the screen inside the tube (3),                                                                          " & vbCrLf
        ws &= "//    the x offset of the screen inside the tube relative to top left of the tube (4),                                       " & vbCrLf
        ws &= "//    the y offset of the screen inside the tube relative to top left of the tube (5)                                        " & vbCrLf
        ws &= "//                                                                                                                           " & vbCrLf
        ws &= "//  on a 320x170 TFT display in landscape this will allow a maximum of:                                                      " & vbCrLf
        ws &= "//    small   10 tubes per row, 2 rows per display                                                                           " & vbCrLf
        ws &= "//    medium:  8 tubes per row, 1 row per display                                                                            " & vbCrLf
        ws &= "//    medium:  5 tubes per row, 1 row per display                                                                            " & vbCrLf
        ws &= "" & vbCrLf
        ws &= "int dimensions[numberOfSizes][numberOfdimensions] = {" & vbCrLf
        ws &= "    { " & smallTubeSize.Width & ", " & smallTubeSize.Height & ", " & smallScreenSize.Width & ", " & smallScreenSize.Height & ", " & smallScreenOffsetWithinTube.X & ", " & smallScreenOffsetWithinTube.Y & " }," & vbCrLf
        ws &= "    { " & mediumTubeSize.Width & ", " & mediumTubeSize.Height & ", " & mediumScreenSize.Width & ", " & mediumScreenSize.Height & ", " & mediumScreenOffsetWithinTube.X & ", " & mediumScreenOffsetWithinTube.Y & " }," & vbCrLf
        ws &= "    { " & largeTubeSize.Width & ", " & largeTubeSize.Height & ", " & largeScreenSize.Width & ", " & largeScreenSize.Height & ", " & largeScreenOffsetWithinTube.X & ", " & largeScreenOffsetWithinTube.Y & " }" & vbCrLf
        ws &= "  };" & vbCrLf
        ws &= vbCrLf

        If File.Exists(ArduinoNixieTubeLibraryName & "supportedValuesAndDimensions.h") Then
            File.Delete(ArduinoNixieTubeLibraryName & "supportedValuesAndDimensions.h")
        End If

        IO.File.WriteAllText(ArduinoNixieTubeLibraryName & "supportedValuesAndDimensions.h", ws)

        Beep()

    End Sub

#End Region

    Private Sub Go_Click(sender As Object, e As EventArgs) Handles btnCreateScreens.Click

        ' Disable the control while processing is underway

        btnCreateScreens.Enabled = False

        CreateAllScreens()
        CreateAllHeaderFiles()

        btnCreateScreens.Enabled = True

    End Sub

End Class

