# -*- coding: utf-8 -*-
"""
Created on Thu Sep 26 09:44:15 2024

@author: anoel
"""
import cv2
import numpy as np
from urllib.request import urlopen

print(cv2.__version__)

# Dimensions de l'image
width, height = 850, 474

# Créer une image verte (intensité maximale)
green_image = np.zeros((height, width, 3), dtype=np.uint8)
green_image[:, :, 1] = 255  # Maximum de vert

# Créer une image bleue (intensité maximale)
blue_image = np.zeros((height, width, 3), dtype=np.uint8)
blue_image[:, :, 0] = 255  # Maximum de bleu

# Créer une image jaune (rouge et vert à intensité maximale)
yellow_image = np.zeros((height, width, 3), dtype=np.uint8)
yellow_image[:, :, 0] = 0    # Pas de bleu
yellow_image[:, :, 1] = 255  # Maximum de vert
yellow_image[:, :, 2] = 255  # Maximum de rouge

req = urlopen('http://www.vgies.com/downloads/robocup.png')
arr = np.asarray(bytearray(req.read()), dtype=np.uint8)
img = cv2.imdecode(arr, -1)
cv2.imshow('RoboCup␣image', img)
cv2.waitKey(0)

# B, G, R = cv2.split(img)
# cv2.imshow("original", img)
# cv2.waitKey(0)
# cv2.imshow("blue", B)
# cv2.waitKey(0)
# cv2.imshow("Green", G)
# cv2.waitKey(0)
# cv2.imshow("Red", R)
# cv2.waitKey(0)

#converting the image to HSV color space using cvtColor function
imagehsv = cv2.cvtColor(img, cv2.COLOR_BGR2HSV)
H, S, V = cv2.split(imagehsv)
# cv2.imshow("Hue", H)
# cv2.waitKey(0)
# cv2.imshow("Saturation", S)
# cv2.waitKey(0)
# cv2.imshow("Value", V)
# cv2.waitKey(0)

#Definition des limites basses et hautes de la couleur jaune en HSV
#A noter que le jaune se situe vers les 25 degres dans la roue de couleur HSV en H
lower_yellow = np.array([20, 100, 100])
upper_yellow = np.array([30,255,255])
#Masquage de l’image HSV pour ne garder que les zones jaunes
imagemaskyellow = cv2.inRange(imagehsv, lower_yellow, upper_yellow)
yellow_result = cv2.bitwise_and(yellow_image, yellow_image, mask=imagemaskyellow)
cv2.imshow("Image␣Masque␣Jaune", yellow_result)
cv2.waitKey(0)


lower_green = np.array([45,50,50])
upper_green = np.array([75,255,255])
#Masquage de l’image HSV pour ne garder que les zones jaunes
imagemaskgreen = cv2.inRange(imagehsv, lower_green, upper_green)
green_result = cv2.bitwise_and(green_image, green_image, mask=imagemaskgreen)
#cv2.imshow("Image␣Masque␣Vert", imagemaskgreen)
# cv2.waitKey(0)

lower_dark = np.array([0,0,0])
upper_dark = np.array([180,100,100])
#Masquage de l’image HSV pour ne garder que les zones jaunes
imagemaskdark = cv2.inRange(imagehsv, lower_dark, upper_dark)
Dark_result = cv2.bitwise_and(blue_image, blue_image, mask=imagemaskdark)
#cv2.imshow("Image␣Masque␣Vert", imagemaskdark)
#cv2.waitKey(0)

combined_mask_1 = cv2.bitwise_or(Dark_result, yellow_result)
combined_mask = cv2.bitwise_or(green_result, combined_mask_1)
cv2.imshow("Image␣Masque␣combine", combined_mask )
cv2.waitKey(0)
