import { useState, useEffect } from 'react';
import { MapContainer, Marker, TileLayer, useMap } from 'react-leaflet';
import 'leaflet/dist/leaflet.css';
import { LatLngExpression } from 'leaflet';

const MapUpdater: React.FC<{ position: LatLngExpression }> = ({ position }) => {
    const map = useMap();
    useEffect(() => {
        map.setView(position, map.getZoom());
    }, [position, map]);

    return null;
};


const ISSTracker = () => {
    const [issPosition, setIssPosition] = useState<LatLngExpression>([0, 0]);

    // Function to fetch ISS data from the API
    const fetchISSPosition = async () => {
        try {
            const response = await fetch('http://api.open-notify.org/iss-now.json');
            const data = await response.json();
            const { latitude, longitude } = data.iss_position;
            setIssPosition([parseFloat(latitude), parseFloat(longitude)]);
        } catch (error) {
            console.error('Error fetching ISS data:', error);
        }
    };

    // Use `useEffect` to update the ISS position every 5 seconds
    useEffect(() => {
        fetchISSPosition(); // Initial fetch
        const intervalId = setInterval(fetchISSPosition, 5000); // Fetch every 5 seconds        

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
            <Marker position={issPosition}></Marker>
            <MapUpdater position={issPosition} />
        </MapContainer>
    );
};

export default ISSTracker;
