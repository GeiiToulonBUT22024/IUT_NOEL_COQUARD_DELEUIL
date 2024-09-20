#include "ChipConfig.h"

// DSPIC33EP512MU814 Configuration Bit Settings
#define FCY 60000000

// 'C' source line config statements

// FGS
#pragma config GWRP = OFF               // General Segment Write-Protect bit (General Segment may be written)
#pragma config GSS = OFF                // General Segment Code-Protect bit (General Segment Code protect is disabled)
#pragma config GSSK = OFF               // General Segment Key bits (General Segment Write Protection and Code Protection is Disabled)

// FOSCSEL
#ifdef USE_CRYSTAL_OSCILLATOR
#pragma config FNOSC = PRIPLL           // Oscillator Source Selection bits (Internal Fast RC with PLL (FRCPLL))
#else
#pragma config FNOSC = FRCPLL           // Oscillator Source Selection bits (Internal Fast RC with PLL (FRCPLL))
#endif
#pragma config IESO = ON               // Two-speed Oscillator Start-up Enable bit (Start up with user-selected oscillator source)

// FOSC
#ifdef USE_CRYSTAL_OSCILLATOR
#pragma config POSCMD = HS            // Primary Oscillator Mode Select Bit (HS Crystal Oscillator Mode)
#pragma config OSCIOFNC = OFF           // OSC2 Pin Function Bit (OSC2 is clock output)
#else
#pragma config POSCMD =  NONE            // Primary Oscillator Mode Select Bit (Primary Oscillator disabled)
#pragma config OSCIOFNC = ON           // OSC2 Pin Function Bit (OSC2 is clock output)
#endif

#pragma config IOL1WAY = OFF             // Peripheral pin select configuration (Allow Multiples )
#pragma config FCKSM = CSECMD           // Clock Switching Mode bits (Both Clock switching and Fail-safe Clock Monitor are disabled)

// FWDT
#pragma config WDTPOST = PS32768        // Watchdog Timer Postscaler bits (1:32,768)
#pragma config WDTPRE = PR128           // Watchdog Timer Prescaler bit (1:128)
#pragma config PLLKEN = ON              // PLL Lock Wait Enable bit (Clock switch to PLL source will wait until the PLL lock signal is valid.)
#pragma config WINDIS = OFF             // Watchdog Timer Window Enable bit (Watchdog Timer in Non-Window mode)
#pragma config FWDTEN = OFF             // Watchdog Timer Enable bit (Watchdog timer enabled/disabled by user software)

// FPOR
#pragma config FPWRT = PWR128           // Power-on Reset Timer Value Select bits (128ms)
#pragma config BOREN = ON               // Brown-out Reset (BOR) Detection Enable bit (BOR is enabled)
#pragma config ALTI2C1 = ON             // Alternate I2C pins for I2C1 (ASDA1/ASCK1 pins are selected as the I/O pins for I2C1)
#pragma config ALTI2C2 = ON            // Alternate I2C pins for I2C2 (I2C2 mapped to SDA2/SCL2 pins)

// FICD
#pragma config ICS = PGD2               // ICD Communication Channel Select bits (Communicate on PGEC2 and PGED2)
#pragma config RSTPRI = PF              // Reset Target Vector Select bit (Device will obtain reset instruction from Primary flash)
#pragma config JTAGEN = OFF             // JTAG Enable Bit (JTAG is disabled)

// FAS
#pragma config AWRP = OFF               // Auxiliary Segment Write-protect bit (Aux Flash may be written)
#pragma config APL = OFF                // Auxiliary Segment Code-protect bit (Aux Flash Code protect is disabled)
#pragma config APLK = OFF               // Auxiliary Segment Key bits (Aux Flash Write Protection and Code Protection is Disabled)

// #pragma config statements should precede project file includes.
// Use project enums instead of #define for ON and OFF.

#include <xc.h>

void InitOscillator() {

    //F_IN = 7.37 MHz    
    OSCTUNbits.TUN = 23; // Décalage de 0.375 % par unité -> Freq = 7.37*(1+TUN*0.0375))
    // 23 permet de passer à 8MHz
    //Configuration for 60MIPS (120MHz), en réalité 119.76 MHz cf. feuille calcul Matlab
    PLLFBDbits.PLLDIV = 88; // M=65
    CLKDIVbits.PLLPOST = 0; // 
    CLKDIVbits.PLLPRE = 1; // 
    // Initiate Clock Switch to FRC oscillator with PLL (NOSC=0b001)
    __builtin_write_OSCCONH(0x01);
    __builtin_write_OSCCONL(OSCCON | 0x01);
    // Wait for Clock switch to occur
    while (OSCCONbits.COSC != 0b001);

    ACLKCON3bits.FRCSEL = 1; //1 = FRC is the clock source for APLL
    //0 = Auxiliary Oscillator or Primary Oscillator is the clock source for APLL (determined by ASRCSEL bit)        

    //Calcul cf. feuille Matlab ou au dessus
    ACLKCON3bits.APLLPRE = 0b001; //001 = Divided by 2
    //Division par 2, donc FAREF= F_IN/2 = 4MHz -> Conforme a 3MHz<FAREF<5.5MHz
    ACLKDIV3bits.APLLDIV = 0b111; //111 = 24
    //Multiplication par 24, donc FAVCO=(F_IN/2)*24 = 4*24 = 96MHz  60MHz < FVCO < 120MHz
    ACLKCON3bits.APLLPOST = 0b110; //110 = Divided by 2
    //Division par 2, donc FAVCO=((F_IN/2)*24)/2 = 48MHz  

    ACLKCON3bits.SELACLK = 1; //1 = Auxiliary PLL or oscillator provides the source clock for auxiliary clock divider
    //0 = Primary PLL provides the source clock for auxiliary clock divider

    ACLKCON3bits.ENAPLL = 1;
    while (ACLKCON3bits.APLLCK != 1);
    
    // Wait for PLL to lock
    while (OSCCONbits.LOCK != 1);

}