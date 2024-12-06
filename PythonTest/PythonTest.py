import pyjevois
if pyjevois.pro: import libjevoispro as jevois
else: import libjevois as jevois

import numpy as np
import cv2

# import scipy.linalgrrrrr
import sympy
import math
def get_rotation_matrix(u, a): # u: axis ; a: angle
    return [
        [(u[0]*u[0]) * (1-np.cos(a)) +      np.cos(a), (u[1]*u[0]) * (1-np.cos(a)) - u[2]*np.sin(a),\
              (u[2]*u[0]) * (1-np.cos(a)) + u[1]*np.sin(a)],
        [(u[0]*u[1]) * (1-np.cos(a)) + u[2]*np.sin(a), (u[1]*u[1]) * (1-np.cos(a)) +      np.cos(a),\
             (u[2]*u[1]) * (1-np.cos(a)) - u[0]*np.sin(a)],
        [(u[0]*u[2]) * (1-np.cos(a)) - u[1]*np.sin(a), (u[1]*u[2]) * (1-np.cos(a)) + u[0]*np.sin(a),\
             (u[2]*u[2]) * (1-np.cos(a)) +      np.cos(a)]
    ]
class PythonTest:
    # ################################################################
    def __init__(self):
        jevois.LINFO("PythonTest Constructor")
        self.frame = 0
        self.timer = jevois.Timer("pytest", 100, jevois.LOG_INFO)
        
        self.ASPECT_RATIO = 3 / 4
        self.H_FOV = 65 * math.pi / 180
        self.V_FOV = 2 * math.atan(math.tan(self.H_FOV / 2) * self.ASPECT_RATIO)
        jevois.LINFO("    V_FOV: " + str(self.V_FOV))
        self.counter = 0
        self.index = 0
        
    # #################################################################
    def init(self):
        jevois.LINFO("PythonTest JeVois init")
        
        pc_misc = jevois.ParameterCategory("Miscellaneous Parameters", "")
        self.gray_threshold = jevois.Parameter(self, "Gray Low Threshold","int",\
            "Low Threshold for countours finding", 10, pc_misc)
        self.min_shape_area = jevois.Parameter(self, "Minimal Shape Area","int",\
            "Minimal shape area recognized", 100, pc_misc)
        self.min_centroid_diff = jevois.Parameter(self, "Minimal Centroid Distance","int",\
            "Minimal distance between centroids", 200, pc_misc)
            
        pc_hcolor = jevois.ParameterCategory("HSV High Color Limit", "")
        pc_lcolor = jevois.ParameterCategory("HSV Low Color Limit", "")
        
        self.hue_lcolor = jevois.Parameter(self, "Hue Low limit","byte",\
            "Hue Low limit", 170, pc_lcolor)
        self.hue_hcolor = jevois.Parameter(self, "Hue High limit","byte",\
            "Hue High limit", 180, pc_hcolor)
        
        self.saturation_lcolor = jevois.Parameter(self, "Saturation Low limit","byte",\
            "Saturation Low limit", 200, pc_lcolor)
        self.saturation_hcolor = jevois.Parameter(self, "Saturation High limit","byte",\
            "Saturation High limit", 255, pc_hcolor)
            
        self.value_lcolor = jevois.Parameter(self, "Value Low limit","byte",\
            "Value Low limit", 100, pc_lcolor)
        self.value_hcolor = jevois.Parameter(self, "Value High limit","byte",\
            "Value High limit", 255, pc_hcolor)
            
        pc_camera = jevois.ParameterCategory("Camera Parameter", "")
        self.camera_posx = jevois.Parameter(self, "Camera Position X (m)","float",\
            "Camera Position X (m)", 0.0, pc_camera)
        self.camera_posy = jevois.Parameter(self, "Camera Position Y (m)","float",\
            "Camera Position Y (m)", 0.0, pc_camera)
        self.camera_posz = jevois.Parameter(self, "Camera Position Z (m)","float",\
            "Camera Position Z (m)", 0.2, pc_camera)
            
        self.camera_pitch_deg = jevois.Parameter(self, "Camera Pitch (°)","float",\
            "Camera Pitch (°)", 52 / 2, pc_camera)
            
            
        self.camera_pos = np.array([self.camera_posx.get(), self.camera_posy.get(), self.camera_posz.get()])
        self.camera_pitch = self.camera_pitch_deg.get() * math.pi / 180
        
        self.x_axis = np.array([1, 0, 0])
        self.y_axis = np.array([0, 1, 0])
        self.z_axis = np.array([0, 0, 1])
        
        self.camera_rotation_M = get_rotation_matrix(self.y_axis, self.camera_pitch)
        
        self.camera_direction = np.dot(self.camera_rotation_M, np.array([1, 0, 0]))
        self.camera_direction = self.camera_direction / np.linalg.norm(self.camera_direction)
        
        
        self.camera_right = self.y_axis # Just because we rotate only on pitch
        self.camera_up = np.cross(self.camera_direction, self.camera_right)
        self.camera_up = self.camera_up / np.linalg.norm(self.camera_up)
        
        
        
            
    # #################################################################
    def findCentroids(self, img, display=False):
        image_hsv = cv2.cvtColor(img, cv2.COLOR_BGR2HSV)
        blank_img = np.zeros((image_hsv.shape[0], image_hsv.shape[1], image_hsv.shape[2]), dtype=np.uint8)
        
        lower_color = np.array([self.hue_lcolor.get(), self.saturation_lcolor.get(), self.value_lcolor.get()])
        upper_color = np.array([self.hue_hcolor.get(), self.saturation_hcolor.get(), self.value_hcolor.get()])
        
        gray = cv2.inRange(image_hsv, lower_color, upper_color)
        
        contours,_ = cv2.findContours(gray, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)

        centroids = []
        for c in contours:
            # Discard small artifact
            if cv2.contourArea(c) < self.min_shape_area.get():
                continue
            
            # Find bounding rect
            x, y, w, h = cv2.boundingRect(c)
            current_centroid = np.array((x + w / 2, y + h))
            
            # Discard too close centroids
            flag_keep = 1
            for temp in centroids:
                if len(centroids) != 0 and np.linalg.norm(current_centroid - temp) < self.min_centroid_diff.get():
                    flag_keep = 0
                    break
                    
            if flag_keep:
                if display:
                    centroids.append(current_centroid)
                    cv2.rectangle(gray, (x, y), (x+w, y+h), (255, 255, 0), 2)
                    cv2.circle(gray, (int(current_centroid[0]), int(current_centroid[1])), 5, (255, 0, 255), -1)
                
        return gray, centroids
        
    # #################################################################

    
    def computePosition(self, centroids, img_width, img_height):
        image_center = np.array([img_width / 2, img_height / 2])
        focal_ref = 772.692
        
        table_position = []
        # ------------------
        # Compute centroids 3D position
        for c in centroids:
            centered_point = np.array((c[0], img_height - c[1])) - image_center
            
            dist_alpha = np.linalg.norm(centered_point)
            
            alpha = np.arctan(dist_alpha / focal_ref)
            theta = math.pi - np.arccos(centered_point[0] / dist_alpha)
            if centered_point[1] < 0:
                theta = -theta
            
            
            rotation_Mroll = get_rotation_matrix(self.camera_direction, theta)
            rotation_Myaw = get_rotation_matrix(self.camera_up, alpha)
            
            obj_direction = np.dot(rotation_Myaw, self.camera_direction)
            obj_direction = np.dot(rotation_Mroll, obj_direction)
            
            step = -self.camera_pos[2] / obj_direction[2]
            y = self.camera_pos[1] + obj_direction[1] * step
            x = self.camera_pos[0] + obj_direction[0] * step
            centroid_relative_tpos = np.array([x, y])
            
            table_position.append(centroid_relative_tpos)
        
        return table_position
    
    # #################################################################
    def processGUI(self, inframe, helper):
        idle, winw, winh = helper.startFrame()
        
        self.timer.start()
        # ---------------------------------
        
        final_image, centroids = self.findCentroids(inframe.getCvBGR(), display=True)
        
        table_pos = self.computePosition(centroids, final_image.shape[1], final_image.shape[0])
        
        # Draw Centroids and table positions
        for c, p in zip(centroids, table_pos):
            cv2.circle(final_image, (int(c[0]), int(c[1])), 5, (255, 0, 0), -1)   
            cv2.putText(final_image, "3D x:" + str(round(p[0], 3)) + " ; y:" + str(round(p[1], 3)), \
                (int(c[0] - 50), int(c[1] + 50)), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (255, 255, 255), 2)
            # jevois.sendSerial(f"x: {round(p[0], 3)} , y: {round(p[1], 3)}")
            # jevois.sendSerial("test")
            helper.drawImage("img", final_image, True, 0, 0, 0, 0,False, False)
        # ---------------------------------
        fps = self.timer.stop()
        helper.iinfo(inframe, fps, winw, winh)
        helper.endFrame()
        
    # #################################################################















































