/* 
 * File:   Robot.h
 * Author: Table2
 *
 * Created on 13 septembre 2023, 15:27
 */

#ifndef ROBOT_H
#define ROBOT_H



typedef struct robotStateBITS {

    union {

        struct {
            unsigned char taskEnCours;
            float vitesseGaucheConsigne;
            float vitesseGaucheCommandeCourante;
            float vitesseDroiteConsigne;
            float vitesseDroiteCommandeCourante;
            float acceleration;
            
            int distanceTelemetreDroit  ;
            int distanceTelemetreCentre  ;
            int distanceTelemetreGauche  ;
            int distanceTelemetreExtremeDroite  ;
            int distanceTelemetreExtremeGauche  ;
            
            double vitesseDroitFromOdometry ;
            double vitesseGaucheFromOdometry ;
            double vitesseLineaireFromOdometry ;
            double vitesseAngulaireFromOdometry ;
            double angleRadianFromOdometry ;
            double xPosFromOdometry_1 ;
            double xPosFromOdometry ;
            double yPosFromOdometry_1 ;
            double yPosFromOdometry ;
            double angleRadianFromOdometry_1 ;
            
        };
    };
} ROBOT_STATE_BITS;
extern volatile ROBOT_STATE_BITS robotState;
#endif /* ROBOT_H */
