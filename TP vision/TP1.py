# -*- coding: utf-8 -*-
"""
Created on Thu Sep 26 09:44:15 2024

@author: anoel
"""
from matplotlib import pyplot as plt
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
# cv2.imshow('RoboCup␣image', img)
# cv2.waitKey(0)

# B, G, R = cv2.split(img)
# cv2.imshow("original", img)
# cv2.waitKey(0)
# cv2.imshow("blue", B)
# cv2.waitKey(0)
# cv2.imshow("Green", G)
# cv2.waitKey(0)
# cv2.imshow("Red", R)
# cv2.waitKey(0)

# converting the image to HSV color space using cvtColor function
imagehsv = cv2.cvtColor(img, cv2.COLOR_BGR2HSV)
H, S, V = cv2.split(imagehsv)
# cv2.imshow("Hue", H)
# cv2.waitKey(0)
# cv2.imshow("Saturation", S)
# cv2.waitKey(0)
# cv2.imshow("Value", V)
# cv2.waitKey(0)

# Definition des limites basses et hautes de la couleur jaune en HSV
# A noter que le jaune se situe vers les 25 degres dans la roue de couleur HSV en H
lower_yellow = np.array([20, 100, 100])
upper_yellow = np.array([30, 255, 255])
# Masquage de l’image HSV pour ne garder que les zones jaunes
imagemaskyellow = cv2.inRange(imagehsv, lower_yellow, upper_yellow)
yellow_result = cv2.bitwise_and(
    yellow_image, yellow_image, mask=imagemaskyellow)
# cv2.imshow("Image␣Masque␣Jaune", yellow_result)
# cv2.waitKey(0)


lower_green = np.array([45, 50, 50])
upper_green = np.array([75, 255, 255])
# Masquage de l’image HSV pour ne garder que les zones jaunes
imagemaskgreen = cv2.inRange(imagehsv, lower_green, upper_green)
green_result = cv2.bitwise_and(green_image, green_image, mask=imagemaskgreen)
# cv2.imshow("Image␣Masque␣Vert", imagemaskgreen)
# cv2.waitKey(0)

lower_dark = np.array([0, 0, 0])
upper_dark = np.array([180, 100, 100])
# Masquage de l’image HSV pour ne garder que les zones jaunes
imagemaskdark = cv2.inRange(imagehsv, lower_dark, upper_dark)
Dark_result = cv2.bitwise_and(blue_image, blue_image, mask=imagemaskdark)
# cv2.imshow("Image␣Masque␣Vert", imagemaskdark)
# cv2.waitKey(0)

combined_mask_1 = cv2.bitwise_or(Dark_result, yellow_result)
combined_mask = cv2.bitwise_or(green_result, combined_mask_1)
# cv2.imshow("Image␣Masque␣combine", combined_mask)
# cv2.waitKey(0)


# TRANSFORMATIONS MANUELLES D’UNE IMAGE
height = img.shape[0]
width = img.shape[1]
channels = img.shape[2]
imgTransform = img
center_coordinates = (width // 2, height // 2)  # Centre de l'image

# Centre et rayon du cercle
center_coordinates = (width // 2, height // 2)  # Centre de l'image
radius = 100  # Rayon du cercle
color = (0, 0, 255)  # Couleur rouge (BGR)
alpha = 0.5  # Transparence du cercle (50%)

# for x in range(0, (int)(width)):
#     grad = (x/width)
#     for y in range(0, (int)(height)):
#         # distance_to_center = np.sqrt(
#         #     (x - center_coordinates[0])**2 + (y - center_coordinates[1])**2)
#         # if distance_to_center <= radius:
#         #     imgTransform[y, x] = (
#         # imgTransform[y, x] * (1 - alpha) + np.array(color) * alpha).astype(np.uint8)

#         imgTransform[y, x][0] *= x/width  # B
#         imgTransform[y, x][1] *= x/width  # G
#         imgTransform[y, x][2] *= x/width  # R

# cv2.imshow("Transformation manuelle de l’image", imgTransform)
# cv2.waitKey(0)

# Conversion de l’image en niveaux de gris
imageGray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
cv2.imshow('Grayscale', imageGray)
# Calcul de l’histogramme
hist, bins = np.histogram(imageGray.flatten(), 256, [0, 256])
hist_H, bins = np.histogram(H.flatten(), 256, [0, 256])
hist_S, bins = np.histogram(S.flatten(), 256, [0, 256])
hist_V, bins = np.histogram(V.flatten(), 256, [0, 256])

# Calcul de l’histogramme cumule
cdf = hist.cumsum()
cdf_normalized = cdf * float(hist.max()) / cdf.max()

cdfH = hist_H.cumsum()
cdfH_normalized = cdfH * float(hist_H.max()) / cdfH.max()

cdfS = hist_S.cumsum()
cdfS_normalized = cdfS * float(hist_S.max()) / cdfS.max()

cdfV = hist_V.cumsum()
cdfV_normalized = cdfV * float(hist_V.max()) / cdfV.max()

# Affichage de l’histogramme cumule en bleu
plt.plot(cdf_normalized, color="r")
plt.plot(cdfH_normalized, color="b")
plt.plot(cdfS_normalized, color="purple")
plt.plot(cdfV_normalized, color="green")
# Affichage de l’histogramme en rouge
plt.hist(imageGray.flatten(), 256, [0, 256], color='r')
plt.hist(H.flatten(), 256, [0, 256], color='b')
plt.hist(S.flatten(), 256, [0, 256], color='black')
plt.hist(V.flatten(), 256, [0, 256], color='green')
plt.xlim([0, 256])
plt.legend(('cdf', 'histogram'), loc='upper left')
plt.show()

# Egalisation de l’histogramme
imgEqu = cv2.equalizeHist(imageGray)
HEqu = cv2.equalizeHist(H)
SEqu = cv2.equalizeHist(S)
VEqu = cv2.equalizeHist(V)
# Calcul de l’histogramme egalise
histEq, binsEq = np.histogram(imgEqu.flatten(), 256, [0, 256])
hist_HEq, bins = np.histogram(HEqu.flatten(), 256, [0, 256])
hist_SEq, bins = np.histogram(SEqu.flatten(), 256, [0, 256])
hist_VEq, bins = np.histogram(VEqu.flatten(), 256, [0, 256])
# Calcul de l’histogramme écumul éégalis
cdfEq = histEq.cumsum()
cdfEq_normalized = cdfEq * float(histEq.max()) / cdfEq.max()


cdfHEq = hist_HEq.cumsum()
cdfHEq_normalized = cdfHEq * float(hist_HEq.max()) / cdfHEq.max()

cdfSEq = hist_SEq.cumsum()
cdfSEq_normalized = cdfSEq * float(hist_SEq.max()) / cdfSEq.max()

cdfVEq = hist_VEq.cumsum()
cdfVEq_normalized = cdfVEq * float(hist_VEq.max()) / cdfVEq.max()

# Affichage de l’image egalisee
cv2.imshow('Image égalise', imgEqu)
cv2.waitKey(0)
plt.clf()

# Affichage de l’histogramme egalise cumule en bleu
plt.plot(cdfEq_normalized, color='r')
plt.plot(cdfHEq_normalized, color='b')
plt.plot(cdfSEq_normalized, color='purple')
plt.plot(cdfVEq_normalized, color='green')
# Affichage de l’histogramme egalise en rouge
plt.hist(imgEqu.flatten(), 256, [0, 256], color='r')
plt.hist(hist_HEq.flatten(), 256, [0, 256], color='b')
plt.hist(hist_SEq.flatten(), 256, [0, 256], color='black')
plt.hist(hist_VEq.flatten(), 256, [0, 256], color='green')
plt.xlim([0, 256])
plt.legend(('cdfEq', 'histogramEq'), loc='upper left')
plt.show()
cv2.waitKey(0)

cv2.imshow("Hue", H)
cv2.waitKey(0)
cv2.imshow("Saturation", S)
cv2.waitKey(0)
cv2.imshow("Value", V)
cv2.waitKey(0)

# cv2.imshow("HEQ", SEqu)
# cv2.waitKey(0)
# blank = np.zeros(
#     (imagehsv.shape[0], imagehsv.shape[1], imagehsv.shape[2]), dtype=np.uint8)
# blankHSV = cv2.cvtColor(blank, cv2.COLOR_BGR2HSV)

# height = blankHSV.shape[0]
# width = blankHSV.shape[1]

# for x in range(0, (int)(width)):
#     for y in range(0, (int)(height)):
#         blankHSV[y, x][0] = HEqu[y, x]
#         blankHSV[y, x][1] = SEqu[y, x]
#         blankHSV[y, x][2] = VEqu[y, x]

# cv2.imshow("top", blankHSV)
