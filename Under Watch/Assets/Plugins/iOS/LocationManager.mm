//
//  LocationManager.mm
//  LocationManagerPlugin
//
//  Created by Mayank Gupta on 24/07/17.
//  Copyright (c) 2017 Mayank Gupta. All rights reserved.
//

#import "LocationManager.h"

@interface LocationManager() {
    CLLocationManager *locationManager;
    NSString *gameObjectName;
    NSString *methodName;
    CLLocation *currentLocation;
    UnityAppController *objectUnityAppController;
    MKMapView *myMapView;
    MKPointAnnotation *annotationPoint;
    BOOL isMapAddedForDeviceCurrentLocation;
}
 
@end

@implementation LocationManager
#pragma mark Unity bridge

+ (LocationManager *)pluginSharedInstance {
    static LocationManager *sharedInstance = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        sharedInstance = [[LocationManager alloc] init];
        
    });
    return sharedInstance;
}

#pragma mark Ios Methods

    // kCLAuthorizationStatusNotDetermined = 0
    // kCLAuthorizationStatusRestricted = 1 
    // kCLAuthorizationStatusDenied = 2
    // kCLAuthorizationStatusAuthorizedAlways = 3 
    // kCLAuthorizationStatusAuthorizedWhenInUse = 4
#pragma mark AuthrizationMethods

-(int) getAuthrizationLevelForApplication {
    CLAuthorizationStatus authorizationStatus = [CLLocationManager authorizationStatus];
    return authorizationStatus;
}

-(void) requestAuthorizedAlways {
    [self initializeLocalManager];
    [locationManager requestAlwaysAuthorization] ;
}

-(void) requestAuthorizedWhenInUse {
    [self initializeLocalManager];
    [locationManager requestWhenInUseAuthorization] ;
}

-(void)showAlertForPermissionsWithTitle:(NSString *)alertTitle 
                                Message:(NSString *)alertMessage
                     DefaultButtonTitle:(NSString *)defaultBtnTitle
                   AndCancelButtonTitle:(NSString *)cancelBtnTitle{
    UIAlertController * alert=[UIAlertController alertControllerWithTitle:alertTitle
                                                                  message:alertMessage
                                                           preferredStyle:UIAlertControllerStyleAlert];

    UIAlertAction* settingButton = [UIAlertAction actionWithTitle:defaultBtnTitle
                                                            style:UIAlertActionStyleDefault
                                                          handler:^(UIAlertAction * action) {
        NSURL *settingsURL = [NSURL URLWithString:UIApplicationOpenSettingsURLString];
        [[UIApplication sharedApplication] openURL: settingsURL
                                           options: [[NSDictionary alloc] init]
                                 completionHandler: nil];
//        [[UIApplication sharedApplication] openURL:settingsURL];
    }];

    UIAlertAction* cancelButton = [UIAlertAction actionWithTitle:cancelBtnTitle
                                                           style:UIAlertActionStyleDefault
                                                         handler:^(UIAlertAction * action) {
        ;
    }];

    [alert addAction:settingButton];
    [alert addAction:cancelButton];
    objectUnityAppController = GetAppController();
    if(objectUnityAppController.rootView == nil)
        return;
    else{
        [objectUnityAppController.rootViewController presentViewController:alert animated:YES completion:nil];
    }
}

#pragma mark LocationMonitoringMethods

-(void) initializeLocalManager {
    if (locationManager == nil) {
        locationManager = [[CLLocationManager alloc] init];
    }
}

-(bool) startLocationMonitoring {
    [self initializeLocalManager];
    locationManager.delegate = self;
    locationManager.desiredAccuracy = kCLLocationAccuracyBest;
    CLAuthorizationStatus authorizationStatus = (CLAuthorizationStatus)[self getAuthrizationLevelForApplication];
    if ((authorizationStatus == kCLAuthorizationStatusAuthorizedAlways) || (authorizationStatus == kCLAuthorizationStatusAuthorizedWhenInUse)) {
        [locationManager startUpdatingLocation];
        return true;
    } else {
        return false;
    }
}

-(void) stopLocationMonitoring {
    if (locationManager == nil)
        return;
    [locationManager stopUpdatingLocation];
}

#pragma mark - CLLocationManagerDelegate

- (void)locationManager:(CLLocationManager *)manager didFailWithError:(NSError *)error {
    NSString *errorString = [NSString stringWithFormat:@"LocationError/%@",error];
            [self sendMessageToUnity:gameObjectName
                                    :methodName
                                    :errorString];
}

- (void)locationManager:(CLLocationManager *)manager didUpdateToLocation:(CLLocation *)newLocation fromLocation:(CLLocation *)oldLocation {
    NSLog(@"didUpdateToLocation: %@", newLocation);
    currentLocation = newLocation;
    
    if (currentLocation != nil) {
        NSString *currentLocationLongitudeString = [NSString stringWithFormat:@"%.8f", currentLocation.coordinate.longitude];
        NSString *currentLocationLatitudeString = [NSString stringWithFormat:@"%.8f", currentLocation.coordinate.latitude];
        NSString *oldLocationLongitudeString = [NSString stringWithFormat:@"%.8f", oldLocation.coordinate.longitude];
        NSString *oldLocationLatitudeString = [NSString stringWithFormat:@"%.8f", oldLocation.coordinate.latitude];
        NSString *message = [NSString stringWithFormat:@"Location/%@/%@/%@/%@",currentLocationLatitudeString,currentLocationLongitudeString,oldLocationLatitudeString,oldLocationLongitudeString];
        if (isMapAddedForDeviceCurrentLocation == YES) {
            [self setLocationOnMap:myMapView :currentLocation :300 :300];
        }
        [self sendMessageToUnity:gameObjectName
                                :methodName
                                :message];
    }
}

#pragma mark - MessageReceivingObjects

-(void)setMessageReceivingObjectName:(NSString*)gameObjectNameTemp
                       AndMethodName:(NSString*)methodNameTemp {
    gameObjectName = gameObjectNameTemp;
    methodName = methodNameTemp;
}

-(void)sendMessageToUnity:(NSString*)gameObjectNameTemp
                         :(NSString*)methodNameTemp
                         :(NSString*)messageTemp {
    const char *message = [messageTemp cStringUsingEncoding:NSASCIIStringEncoding];
    const char *objName = [gameObjectNameTemp cStringUsingEncoding:NSASCIIStringEncoding];
    const char *methodTemp = [methodNameTemp cStringUsingEncoding:NSASCIIStringEncoding];
    UnitySendMessage(objName, methodTemp, message);

}

#pragma mark - GeoCoderMethods

-(void)getAddressForCurrentLocation {
    [self getAddressForLocation: currentLocation];
}

-(void)getAddressForLocationWithLatitude:(NSString *) locationLatitudeTemp 
                            AndLongitude:(NSString *) locationLongitudeTemp {
    CLLocation *customizedLocation = [self getLocationFromLatitude:locationLatitudeTemp
                                                       AndLonitude:locationLongitudeTemp];
    [self getAddressForLocation:customizedLocation];
}

-(void)getAddressForLocation:(CLLocation *)locationForAddress {
    CLGeocoder *geocoder = [[CLGeocoder alloc] init];
    [geocoder reverseGeocodeLocation:locationForAddress completionHandler:^(NSArray *placemarks, NSError *error) {
        if (error == nil && [placemarks count] > 0) {
            CLPlacemark *placemark = [placemarks lastObject];
            NSString *address = [NSString stringWithFormat:@"Address/%@/%@/%@/%@/%@/%@",
                                 placemark.subThoroughfare, placemark.thoroughfare,
                                 placemark.postalCode, placemark.locality,
                                 placemark.administrativeArea,
                                 placemark.country];
            [self sendMessageToUnity:gameObjectName
                                    :methodName
                                    :address];
        } else {
            NSString *errorString = [NSString stringWithFormat:@"AddressError/%@",error.debugDescription];
            [self sendMessageToUnity:gameObjectName
                                    :methodName
                                    :errorString];
        }
    } ];
}

#pragma mark - Open Maps Applications

-(void) openMapsForCurrentLocation {
    [self openMapsAppWithLocation:currentLocation];
}

-(void) openMapsWithLocationWithLatitude:(NSString *) locationLatitudeTemp
                            AndLongitude:(NSString *) locationLongitudeTemp {
    Class mapItemClass = [MKMapItem class];
    if (mapItemClass && [mapItemClass respondsToSelector:@selector(openMapsWithItems:launchOptions:)])
    {
        CLLocation *customizedLocation = [self getLocationFromLatitude:locationLatitudeTemp
                                                           AndLonitude:locationLongitudeTemp];
        [self openMapsAppWithLocation:customizedLocation];
    }
}

-(void) openMapsAppWithLocation: (CLLocation *) location {
    CLLocationCoordinate2D coordinate = location.coordinate;
    MKPlacemark *placemark = [[MKPlacemark alloc] initWithCoordinate:coordinate
                                            addressDictionary:nil];
    MKMapItem *mapItem = [[MKMapItem alloc] initWithPlacemark:placemark];
    [mapItem openInMapsWithLaunchOptions:nil];
}

-(void) openAppleMapsAppWithDrivingWithLatitude:(NSString *) locationLatitudeTemp
                                   AndLongitude:(NSString *) locationLongitudeTemp {
    CLLocation *location = [self getLocationFromLatitude:locationLatitudeTemp
                                             AndLonitude:locationLongitudeTemp];
    CLLocationCoordinate2D coordinate = location.coordinate;
    MKPlacemark *placemark = [[MKPlacemark alloc] initWithCoordinate:coordinate
                                            addressDictionary:nil];
    MKMapItem *mapItem = [[MKMapItem alloc] initWithPlacemark:placemark];
    NSDictionary *launchOption = @{
        MKLaunchOptionsDirectionsModeKey: MKLaunchOptionsDirectionsModeDriving
    };
    [mapItem openInMapsWithLaunchOptions:launchOption];
}

#pragma mark - Utility Method
-(CLLocation *) getLocationFromLatitude:(NSString *) locationLatitudeTemp
                            AndLonitude:(NSString *) locationLongitudeTemp {
    NSNumberFormatter *numberFormatter = [[NSNumberFormatter alloc] init];
    numberFormatter.numberStyle = NSNumberFormatterDecimalStyle;
    double locationLatitude = [numberFormatter numberFromString:locationLatitudeTemp].doubleValue;
    double locationLongitude = [numberFormatter numberFromString:locationLongitudeTemp].doubleValue;
    CLLocation *customizedLocation = [[CLLocation alloc] initWithLatitude:locationLatitude longitude:locationLongitude];
    return customizedLocation;
}



#pragma mark - Open Maps Inside App
-(void) openMapViewWithCurrentLocation: (NSString *)origin_x
                                      : (NSString *)origin_y
                                      : (NSString *)width
                                      : (NSString *)height {
    [self addMapToViewSubView:origin_x :origin_y :width :height];
    [self setLocationOnMap:myMapView :currentLocation :300 :300];
    isMapAddedForDeviceCurrentLocation = YES;
}

-(void) openMapViewWithLatitude: (NSString *) locationLatitudeTemp
                    AndLonitude: (NSString *) locationLongitudeTemp
                        originx: (NSString *)origin_x
                        originy: (NSString *)origin_y
                          width: (NSString *)width
                         height: (NSString *)height{
    [self addMapToViewSubView:origin_x :origin_y :width :height];
    CLLocation *customizedLocation = [self getLocationFromLatitude:locationLatitudeTemp
                                                           AndLonitude:locationLongitudeTemp];
    [self setLocationOnMap:myMapView :customizedLocation :300 :300];
    isMapAddedForDeviceCurrentLocation = NO;
}

-(void) addMapToViewSubView : (NSString *)origin_x
                            : (NSString *)origin_y
                            : (NSString *)width
                            : (NSString *)height {
    if (myMapView == nil) {
        myMapView = [[MKMapView alloc] init];
    } else {
        [myMapView removeFromSuperview];
    }
    NSNumberFormatter *numberFormatter = [[NSNumberFormatter alloc] init];
    numberFormatter.numberStyle = NSNumberFormatterDecimalStyle;
    
    myMapView.frame = CGRectMake([numberFormatter numberFromString:origin_x].doubleValue,
                                 [numberFormatter numberFromString:origin_y].doubleValue,
                                 [numberFormatter numberFromString:width].doubleValue,
                                 [numberFormatter numberFromString:height].doubleValue);
    objectUnityAppController = GetAppController();
    if(objectUnityAppController.rootView == nil)
        return;
    else{
        [objectUnityAppController.rootView addSubview:myMapView];
    }
    [myMapView setHidden:false];
}

- (void) hideMap : (BOOL)state {
    if (myMapView != nil) {
        [myMapView setHidden:state];
    }
}

- (void) setLocationOnMap : (MKMapView *) mapView
                          : (CLLocation *) location
                          : (double) latArea
                          : (double) longArea {
    
    if (mapView == nil || mapView.isHidden == true) {
        return;
    }
    
    MKCoordinateRegion region = MKCoordinateRegionMakeWithDistance(location.coordinate, latArea, longArea);
    [myMapView setRegion:[myMapView regionThatFits:region] animated:YES];
    
    if (annotationPoint == nil) {
        annotationPoint = [[MKPointAnnotation alloc] init];
    }
    annotationPoint.coordinate = location.coordinate;
    [myMapView addAnnotation:annotationPoint];
}

@end

// Helper method used to convert NSStrings into C-style strings.
NSString *CreateStr(const char* string) {
    if (string) {
        return [NSString stringWithUTF8String:string];
    } else {
        return [NSString stringWithUTF8String:""];
    }
}


// Unity can only talk directly to C code so use these method calls as wrappers
// into the actual plugin logic.
extern "C" {

    int _getAuthrizationLevelForApplication() {
        LocationManager *locationManager = [LocationManager pluginSharedInstance];
        return [locationManager getAuthrizationLevelForApplication];
    }

    void _requestAuthorizedAlways() {
        LocationManager *locationManager = [LocationManager pluginSharedInstance];
        [locationManager requestAuthorizedAlways];
    }

    void _requestAuthorizedWhenInUse() {
        LocationManager *locationManager = [LocationManager pluginSharedInstance];
        [locationManager requestAuthorizedWhenInUse];
    }

    bool _startLocationMonitoring() {
        LocationManager *locationManager = [LocationManager pluginSharedInstance];
        return [locationManager startLocationMonitoring];
    }

    void _stopLocationMonitoring() {
        LocationManager *locationManager = [LocationManager pluginSharedInstance];
        [locationManager stopLocationMonitoring];
    }

    void _setMessageReceivingObjectName(const char *gameObjectNameTemp,const char *methodNameTemp) {
        LocationManager *locationManager = [LocationManager pluginSharedInstance];
        [locationManager setMessageReceivingObjectName:CreateStr(gameObjectNameTemp)
                                   AndMethodName:CreateStr(methodNameTemp)];
    }

    void _getAddressForCurrentLocation() {
        LocationManager *locationManager = [LocationManager pluginSharedInstance];
        [locationManager getAddressForCurrentLocation];
    }

    void _getAddressForLocationWithLatitudeLongitude(const char *locationLatitudeTemp,const char *locationLongitudeTemp) {
        LocationManager *locationManager = [LocationManager pluginSharedInstance];
        [locationManager getAddressForLocationWithLatitude:CreateStr(locationLatitudeTemp)
                                              AndLongitude:CreateStr(locationLongitudeTemp)];
    }


    void _showAlertForPermissions(const char *alertTitle,const char *alertMessage,const char *defaultBtnTitle,const char *cancelBtnTitle) {
        LocationManager *locationManager = [LocationManager pluginSharedInstance];
        [locationManager showAlertForPermissionsWithTitle:CreateStr(alertTitle) 
                                                  Message:CreateStr(alertMessage)
                                       DefaultButtonTitle:CreateStr(defaultBtnTitle)
                                     AndCancelButtonTitle:CreateStr(cancelBtnTitle)];
    }

    void _openAppleMapsWithMyCurrentLocation() {
        LocationManager *locationManager = [LocationManager pluginSharedInstance];
        [locationManager openMapsForCurrentLocation];
    }

    void _openAppleMapsWithLatitudeLongitude(const char *locationLatitudeTemp,const char *locationLongitudeTemp) {
        LocationManager *locationManager = [LocationManager pluginSharedInstance];
        [locationManager openMapsWithLocationWithLatitude:CreateStr(locationLatitudeTemp)
                                             AndLongitude:CreateStr(locationLongitudeTemp)];
    }
    
    void _openAppleMapsWithDrivingMode(const char *locationLatitudeTemp,const char *locationLongitudeTemp) {
        LocationManager *locationManager = [LocationManager pluginSharedInstance];
        [locationManager openAppleMapsAppWithDrivingWithLatitude:CreateStr(locationLatitudeTemp)
                                                    AndLongitude:CreateStr(locationLongitudeTemp)];
    }

    void _openMapsViewWithCurrentLocation(const char *x_origin,
                                          const char *y_origin,
                                          const char *height,
                                          const char *width) {
        LocationManager *locationManager = [LocationManager pluginSharedInstance];
        [locationManager openMapViewWithCurrentLocation:CreateStr(x_origin)
                                                       :CreateStr(y_origin)
                                                       :CreateStr(width)
                                                       :CreateStr(height)];
    }

    void _openMapsViewWithSpecificLocation(const char *latitude,
                                           const char *longitude,
                                           const char *x_origin,
                                           const char *y_origin,
                                           const char *height,
                                           const char *width) {
        LocationManager *locationManager = [LocationManager pluginSharedInstance];
        [locationManager openMapViewWithLatitude:CreateStr(latitude)
                                     AndLonitude:CreateStr(longitude)
                                         originx:CreateStr(x_origin)
                                         originy:CreateStr(y_origin)
                                           width:CreateStr(width)
                                          height:CreateStr(height)];
    }

    void _showMap() {
        LocationManager *locationManager = [LocationManager pluginSharedInstance];
        [locationManager hideMap:false];
    }

    void _hideMap() {
        LocationManager *locationManager = [LocationManager pluginSharedInstance];
        [locationManager hideMap:true];
    }
}
