import { GeoLocation } from "./geo-location";
import ISSTracker from "./iss-tracker";

export const IssTracketPage: React.FC = () => {


    return <>
        <h1 className="p-4 text-2xl font-bold text-center bg-black text-white flex justify-between">
            <a href="/"
                className="underline text-white hover:text-gray-200 visited:text-white"
                title="Load top news"
            >
               &lt;-  Space News
            </a>
            ISS Tracker
        </h1>
        <main>
            <ISSTracker />
            <GeoLocation />
        </main>
    </>;
}