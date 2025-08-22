import React, { useState, useEffect } from 'react';

// Define a type for the user's location
interface Location {
    latitude: number;
    longitude: number;
}

interface ISSPass {
    risetime: number;  // When the pass starts (Unix timestamp)
    duration: number;  // How long the ISS will be visible (seconds)
}

export const GeoLocation: React.FC = () => {
    const [location, setLocation] = useState<Location | null>(null);
    const [passes, setPasses] = useState<ISSPass[]>([]);
    const [error, setError] = useState<string | null>(null);

    // Function to fetch ISS pass times using Open Notify API
    const fetchISSPasses = async (lat: number, lon: number) => {
        try {
            const response = await fetch(
                `http://api.open-notify.org/iss-pass.json?lat=${lat}&lon=${lon}`
            );
            const data = await response.json();
            if (data && data.response) {
                setPasses(data.response);
            } else {
                setError('Error fetching ISS pass data.');
            }
        } catch (error) {
            setError('Error fetching ISS pass data.');
        }
    };

    // Function to get the user's current location
    const getLocation = () => {
        if (navigator.geolocation) {
            // Use the Geolocation API to get the user's current position
            navigator.geolocation.getCurrentPosition(
                (position) => {
                    // Success callback
                    const { latitude, longitude } = position.coords;
                    setLocation({ latitude, longitude });
                },
                (err) => {
                    // Error callback
                    setError(`Error: ${err.message}`);
                }
            );
        } else {
            // Geolocation is not supported by the browser
            setError("Geolocation is not supported by this browser.");
        }
    };

    const formatTime = (timestamp: number): string => {
        const date = new Date(timestamp * 1000);
        return date.toLocaleString();
    };

    useEffect(() => {
        getLocation();
    }, []);

    useEffect(() => {
        if (location) {
            fetchISSPasses(location.latitude, location.longitude);
        }
    }, [location]);

    return (
        <div>
            <h2>Get Current Location</h2>
            {location ? (
                <p>
                    Your location is: Latitude: {location.latitude}, Longitude: {location.longitude}
                </p>
            ) : error ? (
                <p>{error}</p>
            ) : (
                <p>Fetching your location...</p>
            )}
            {passes.length > 0 ? (
                <ul>
                    {passes.map((pass, index) => (
                        <li key={index}>
                            Pass {index + 1}: Starts at {formatTime(pass.risetime)} for {pass.duration} seconds.
                        </li>
                    ))}
                </ul>
            ) : (
                <p>Loading ISS pass times...</p>
            )}
        </div>
    );
};
