# LoftRES

Ok, maybe this is the most interesting tool for Ravenloft: Strahd's Possession, Ravenloft: Stone Prophet videogames and Menzoberranzan.  
With this tool you can extract/generate resource (RES??) raw files AND you can decrypt/convert, that raw files into .TGA indexed files and viceversa.  
Apart of the .RAW files (the images or other files like voices), you will have ONE RES.TXT file with the list of that images. You will need this for generate again the resource file.
The new converted raw image will be inserted in the RES?? file WITHOUT compression.

<img width="989" height="643" alt="imagen" src="https://github.com/user-attachments/assets/0ab40ec5-3a75-4f7b-aa1f-33469223fc2e" />

The first time you use this tool, for extract, you will get a bunch of files.  
Normally, the first raw files of RES0 (first 14 for Strahd and first 11 for Stone) are the FONTs of the game.  
You will have to check which is the best palette you must use to show the image the most accurate possible.  
I recomend you use the PALETTE feature and have the COLORS file in the same folder where the tool is.  

Once you have all the raw files, you can decrypt them into images. I recommend also in case you want to translate or modify some of the images use the palette feature to show the image correctly.  
After updating one image, we will get a new image with ????_NEW.RAW. We will need to update the original ????.RAW file with this if we want to generate again the resource file.

For example, we want to update image 0 of RES9 (of Strahd's, but this does not matter because this works the same in both games):  
> **"C:\loftres -e RES9"**    (export all the files)  
> **"C:\loftres -d 0000.RAW:4"**  (decrypt 0000.RAW to 0000.TGA using palette 4)  
> We modify 0000.TGA with some graphic tool like Gimp. PLEASE, THE IMAGE MUST BE COLOR INDEXED.  
> **"C:\loftres -r 0000.TGA"**  (convert 0000.TGA to 0000_NEW.RAW)  
> **"C:\copy 0000_NEW.RAW 0000.RAW"**   (copy the new file with the needed name for generating the new resource file)  
> **"C:\loftres -g RES9"**    (this will create RES9_NEW that we will be able to use in the game)  
