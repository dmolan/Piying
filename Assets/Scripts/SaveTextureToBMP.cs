/*
 * Contains functions, needed for converting Texture2D to BMP image format
 */

using UnityEngine;
using System.IO;

public class SaveTextureToBMP : MonoBehaviour
{
    const int BYTES_PER_PIXEL = 3; // red, green, blue
    const int FILE_HEADER_SIZE = 14;
    const int INFO_HEADER_SIZE = 40;



    // By the given "Texture2D" object, saves ".bmp" file to path "imageFileName"
    public void saveTextureToBMP(string imageFileName, Texture2D texture)
    {
        int height = texture.width;
        int width = texture.height;
        byte[,,] image = new byte[width, height, BYTES_PER_PIXEL];

        for (int i = 0; i < height; i++) 
        {
            for (int j = 0; j < width; j++) 
            {
                image[j, i, 2] = (byte)(texture.GetPixel(j, i).r * 255f);
                image[j, i, 1] = (byte)(texture.GetPixel(j, i).g * 255f);
                image[j, i, 0] = (byte)(texture.GetPixel(j, i).b * 255f);
            }
        }

        generateBitmapImage(image, height, width, imageFileName);
    }

    // Not override as "WriteAllBytes", but add new bytes to the end of the file
    public static void appendAllBytes(string path, byte[] bytes)
    {
        using (var stream = new FileStream(path, FileMode.Append))
        {
            stream.Write(bytes, 0, bytes.Length);
        }
    }

    void generateBitmapImage(byte[,,] image, int height, int width, string imageFileName)
    {
        int widthInBytes = width * BYTES_PER_PIXEL;

        byte[] padding = new byte[3]{0, 0, 0};
        int paddingSize = (4 - (widthInBytes) % 4) % 4;

        int stride = (widthInBytes) + paddingSize;

        byte[] fileHeader = createBitmapFileHeader(height, stride);
        System.IO.File.WriteAllBytes(imageFileName, fileHeader);

        byte[] infoHeader = createBitmapInfoHeader(height, width);
        appendAllBytes(imageFileName, infoHeader);

        byte[] pixelData = new byte[height * widthInBytes];
        int indexNow = 0;

        for (int i = 0; i < height; i++) 
        {
            for (int j = 0; j < width; j++) 
            {
                pixelData[indexNow+0] = image[j, i, 0];
                pixelData[indexNow+1] = image[j, i, 1];
                pixelData[indexNow+2] = image[j, i, 2];
                indexNow += 3;
            }
        }
        appendAllBytes(imageFileName, pixelData);
    }

    byte[] createBitmapFileHeader(int height, int stride)
    {
        int fileSize = FILE_HEADER_SIZE + INFO_HEADER_SIZE + (stride * height);

        byte[] fileHeader = 
        {
            0,0,     // signature
            0,0,0,0, // image file size in bytes
            0,0,0,0, // reserved
            0,0,0,0, // start of pixel array
        };

        fileHeader[ 0] = (byte)('B');
        fileHeader[ 1] = (byte)('M');
        fileHeader[ 2] = (byte)(fileSize      );
        fileHeader[ 3] = (byte)(fileSize >>  8);
        fileHeader[ 4] = (byte)(fileSize >> 16);
        fileHeader[ 5] = (byte)(fileSize >> 24);
        fileHeader[10] = (byte)(FILE_HEADER_SIZE + INFO_HEADER_SIZE);

        return fileHeader;
    }

    byte[] createBitmapInfoHeader(int height, int width)
    {
        byte[] infoHeader = 
        {
            0,0,0,0, // header size
            0,0,0,0, // image width
            0,0,0,0, // image height
            0,0,     // number of color planes
            0,0,     // bits per pixel
            0,0,0,0, // compression
            0,0,0,0, // image size
            0,0,0,0, // horizontal resolution
            0,0,0,0, // vertical resolution
            0,0,0,0, // colors in color table
            0,0,0,0, // important color count
        };

        infoHeader[ 0] = (byte)(INFO_HEADER_SIZE);
        infoHeader[ 4] = (byte)(width      );
        infoHeader[ 5] = (byte)(width >>  8);
        infoHeader[ 6] = (byte)(width >> 16);
        infoHeader[ 7] = (byte)(width >> 24);
        infoHeader[ 8] = (byte)(height      );
        infoHeader[ 9] = (byte)(height >>  8);
        infoHeader[10] = (byte)(height >> 16);
        infoHeader[11] = (byte)(height >> 24);
        infoHeader[12] = (byte)(1);
        infoHeader[14] = (byte)(BYTES_PER_PIXEL*8);

        return infoHeader;
    }
}
