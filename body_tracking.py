# Import dependencies
import cv2 as cv
import numpy as np
import mediapipe as mp

import time
import socket

# Draw the landmarks of a body
showContour = True

# Time at the start
pTime = 0

# Define pose estimator
mp_drawing = mp.solutions.drawing_utils
mp_drawing_styles = mp.solutions.drawing_styles
mp_pose = mp.solutions.pose

# Communication
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
serverAddressPort = ("127.0.0.1", 5052) # Address, Port

# Define webcam capture object
cap = cv.VideoCapture(1)
# Frame size parameters
width, height = 1280, 720
# Set video frame size
cap.set(3, width)
cap.set(4, height)

with mp_pose.Pose(
    min_detection_confidence=0.5,
    min_tracking_confidence=0.5) as pose:
    while cap.isOpened():

        # resd the captured video
        ret, frame = cap.read()

        # Calculate FPS rate
        cTime = time.time()
        fps = 1/(cTime - pTime)
        pTime = cTime
        
        # Convert video frame to RGB to process pose estimation
        image = cv.cvtColor(frame, cv.COLOR_BGR2RGB)
        image = cv.flip(image, 1)
        results = pose.process(image)
        # Convert video frame back to BGR
        image = cv.cvtColor(image, cv.COLOR_RGB2BGR)

        # Show a landmark line
        mp_drawing.draw_landmarks(image, results.pose_landmarks, mp_pose.POSE_CONNECTIONS)
        if showContour:
            cv.line(image, (0, int(height * 0.25)), (width, int(height * 0.25)), (255, 255, 255), 2)
            cv.line(image, (0, int(height * 0.60)), (width, int(height * 0.60)), (255, 255, 255), 2)
            cv.line(image, (int(width/2), 0), (int(width/2), height), (255, 255, 255), 2)
            cv.putText(image, "Jump", (600, 90), cv.FONT_HERSHEY_SIMPLEX, 1, (0, 0, 255), 3)
            cv.putText(image, "Run", (600, 306), cv.FONT_HERSHEY_SIMPLEX, 1, (0, 0, 255), 3)
            cv.putText(image, "Slide", (600, 576), cv.FONT_HERSHEY_SIMPLEX, 1, (0, 0, 255), 3)
            # cv.rectangle(image, (600,90), (400,200), (255,0,0), 1)
        
        data = [0, 0, 0, 0]
        # 0 - If the body is detected or not
        # 1 - Detect running; 0 = No running; 1 = Running
        # 2 - Detect motion; 0 = Run; 1 = Jump; 2 = Slide
        # 3 - Check Lane; 0 = Middle; 1 = Right; 2 = Left
        
        if results.pose_landmarks:
            data[0] = 1
            
            body_locations_y_wrist = np.array([
                results.pose_landmarks.landmark[mp_pose.PoseLandmark.LEFT_WRIST].y,
                results.pose_landmarks.landmark[mp_pose.PoseLandmark.RIGHT_WRIST].y,
            ])
            
            # Detect if running 
            if ((body_locations_y_wrist[0] > 0.6) and (body_locations_y_wrist[1] < 0.6)) or ((body_locations_y_wrist[1] > 0.6) and (body_locations_y_wrist[0] < 0.6)):
                print("run")
                data[1] = 1
            else: 
                data[1] = 0

            # Position of body in Y axis
            body_locations_y_shoulder = np.array([
                results.pose_landmarks.landmark[mp_pose.PoseLandmark.LEFT_SHOULDER].y,
                results.pose_landmarks.landmark[mp_pose.PoseLandmark.RIGHT_SHOULDER].y,
            ])

            if all(bodyY < 0.25 for bodyY in body_locations_y_shoulder):
                data[2] = 1     # Jump
                cv.putText(image, "Action: Jump", 
                       (60, 700), cv.FONT_HERSHEY_SIMPLEX, 1, (255, 255, 255), 3)
            elif all(bodyY > 0.60 for bodyY in body_locations_y_shoulder):
                data[2] = 2     # Slide
                cv.putText(image, "Action: Slide", 
                       (60, 700), cv.FONT_HERSHEY_SIMPLEX, 1, (255, 255, 255), 3)
            else:
                data[2] = 0 
            
            if (data[1] == 1) and (data[2] == 0):
                cv.putText(image, "Action: Run", 
                       (60, 700), cv.FONT_HERSHEY_SIMPLEX, 1, (255, 255, 255), 3)
            elif (data[1] == 0) and (data[2] == 0):
                cv.putText(image, "Action: Still", 
                       (60, 700), cv.FONT_HERSHEY_SIMPLEX, 1, (255, 255, 255), 3)
            
            # Position of body in X axis
            body_locations_x_shoulder = np.array([
                results.pose_landmarks.landmark[mp_pose.PoseLandmark.LEFT_SHOULDER].x,
                results.pose_landmarks.landmark[mp_pose.PoseLandmark.RIGHT_SHOULDER].x,
                # results.pose_landmarks.landmark[mp_pose.PoseLandmark.LEFT_HIP].x,
                # results.pose_landmarks.landmark[mp_pose.PoseLandmark.RIGHT_HIP].x
            ])
            
            if all(bodyX < 0.5 for bodyX in body_locations_x_shoulder):
                data[3] = 2
                cv.putText(image, "Lane: Left", 
                       (60, 650), cv.FONT_HERSHEY_SIMPLEX, 1, (255, 255, 255), 3)
            elif all(bodyX > 0.50 for bodyX in body_locations_x_shoulder):
                data[3] = 1
                cv.putText(image, "Lane: Right", 
                       (60, 650), cv.FONT_HERSHEY_SIMPLEX, 1, (255, 255, 255), 3)
            else:
                data[3] = 0
                cv.putText(image, "Lane: Middle", 
                       (60, 650), cv.FONT_HERSHEY_SIMPLEX, 1, (255, 255, 255), 3)
                    
            
            # body_locations_y_wrist = np.array([
            #     results.pose_landmarks.landmark[mp_pose.PoseLandmark.LEFT_WRIST].y,
            #     results.pose_landmarks.landmark[mp_pose.PoseLandmark.RIGHT_WRIST].y,
            # ])
            
            
        
        # Send data to Unity
        print(data)
        sock.sendto(str.encode(str(data)), serverAddressPort)

        # Show a webcam video
        cv.imshow("Webcam Python", image)
        
        # Inverted Video



        # Turn off the program if press 'q'
        if cv.waitKey(1) & 0xFF == ord('q'):
            data = [9]
            sock.sendto(str.encode(str(data)), serverAddressPort)
            break

# Close every window screen opened by OpenCV
cap.release()
cv.destroyAllWindows()