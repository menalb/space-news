import ISSTracker from "./iss-tracker";

export const IssTracketPage: React.FC = () => {


    return <>
        <h1 className="p-4 text-2xl font-bold text-center bg-black text-white flex justify-between">
            <a href="/"
                className="underline text-amber-600 hover:text-amber-800 visited:text-white"
                title="Load top news"
            >
               &lt;-  Space News
            </a>
            ISS Tracker
        </h1>
        <main>
            <ISSTracker />
        </main>
    </>;
}