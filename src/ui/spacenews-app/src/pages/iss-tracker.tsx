import { useState, useEffect } from 'react';
import { MapContainer, Marker, Polyline, TileLayer, useMap } from 'react-leaflet';
import 'leaflet/dist/leaflet.css';
import { LatLngExpression } from 'leaflet';
import L from 'leaflet';
import issImg from '../assets/iss.png';

const MapUpdater: React.FC<{ position: LatLngExpression }> = ({ position }) => {
    const map = useMap();
    useEffect(() => {
        map.setView(position, map.getZoom());
    }, [position, map]);

    return null;
};

const icon = L.icon({
    iconUrl: issImg,
    iconSize: [28, 35]
});

// ISS Position interface
interface ISSPosition {
    latitude: number;
    longitude: number;
}
type LatLng = [number, number];

const generateTimestamps = (): number[] => {
    const now = Math.floor(Date.now() / 1000); // Current time in Unix seconds
    const minutes = 300;
    const timestamps = [];

    for (let i = 0; i <= 40; i++) {
        timestamps.push(now + i * minutes); // Add 60 seconds for each minute
    }

    return timestamps;
};

const ISSTracker = () => {
    const [issPosition, setIssPosition] = useState<LatLngExpression>([0, 0]);
    const [issRoute, setIssRoute] = useState<LatLng[]>([]);

    // Function to fetch the ISS route (array of positions over time)
    const fetchISSRoute = async (): Promise<LatLng[]> => {
        try {
            // Example timestamps (current and future timestamps)
            const timestamps = generateTimestamps();
            const response = await fetch(
                `https://api.wheretheiss.at/v1/satellites/25544/positions?timestamps=${timestamps}`
            );
            const data: ISSPosition[] = await response.json();
            return data.map(pos => [pos.latitude, pos.longitude]);
        } catch (error) {
            console.error('Error fetching ISS route:', error);
            return [];
        }
    };
    
    // Fetch ISS route (positions over time)
    const updateISSRoute = async () => {
        const route = await fetchISSRoute();
        setIssRoute(route);
    };

    // Function to fetch ISS data from the API
    const fetchISSPosition = async () => {
        try {
            const response = await fetch('https://api.wheretheiss.at/v1/satellites/25544');
            const data = await response.json();
            const { latitude, longitude } = data;
            setIssPosition([parseFloat(latitude), parseFloat(longitude)]);
        } catch (error) {
            console.error('Error fetching ISS data:', error);
        }
    };

    // Use `useEffect` to update the ISS position every 5 seconds
    useEffect(() => {
        fetchISSPosition(); // Initial fetch
        updateISSRoute(); // Fetch the route (positions over time)
        const intervalId = setInterval(() => {
            fetchISSPosition();
            updateISSRoute();
        }, 5000); // Fetch every 5 seconds        

        // Cleanup interval on component unmount
        return () => clearInterval(intervalId);
    }, []);

    return (
        <MapContainer
            center={issPosition}
            zoom={3}
            style={{ height: '500px', width: '100%' }}
        >
            <TileLayer
                url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
            />
            <Marker position={issPosition} icon={icon}></Marker>
            <MapUpdater position={issPosition} />
            {/* Polyline to represent the ISS route */}
            {issRoute.length > 0 && <Polyline positions={issRoute} color="navy" weight={0.5} />}
        </MapContainer>
    );
};

export default ISSTracker;
