export const Loader: React.FC<{ isLoading: boolean }> = ({ isLoading }) => <>
    {isLoading && <div className="flex justify-center h-screen mt-20 h-10">
        <div className="relative inline-flex justify-items-start">
            <div className="w-8 h-8 bg-amber-600 rounded-full"></div>
            <div className="w-8 h-8 bg-amber-600 rounded-full absolute top-0 left-0 animate-ping"></div>
            <div className="w-8 h-8 bg-amber-600 rounded-full absolute top-0 left-0 animate-pulse"></div>
        </div>
    </div>
    }
</>;