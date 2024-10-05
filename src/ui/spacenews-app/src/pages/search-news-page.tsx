import { useEffect, useState } from "react";
import { NewsEntry, Source } from "./../data";
import { NewList } from "./../NewsList";
import { Loader } from "./../Loader";
import { SourcesSelection } from "./../SourcesSelection";
import { SummaryComponent } from "./../SummaryComponent";

const apiURL = import.meta.env.VITE_API;
const newsURL = `${apiURL}/news`;

export const SearchNewsPage: React.FC = () => {
    const [news, setNews] = useState<NewsEntry[]>([]);
    const [sources, setSources] = useState<Source[]>([]);
    const [isSearching, setIsSearching] = useState<boolean>(false);
    const [isSourcesVisible, setIsSourcesVisible] = useState<boolean>(false);
    const [searchText, setSearchText] = useState<string>("");

    const getNews = async (sources: string[] = []) => {
        try {
            setIsSearching(true);
            let qs = "";
            if (sources && sources.length > 0) {
                qs = `?${sources.map(s => `sources=${s}&`).join("")}`;
                console.log('qs', qs);
            }
            const response = await fetch(`${newsURL}${qs}`);
            const jsonData = await response.json();
            setNews(jsonData);
        } catch (error) {
            console.error(error);
        }
        setIsSearching(false);
    }

    const getSources = async () => {
        try {
            const response = await fetch(`${apiURL}/sources`);
            const jsonData = await response.json();
            const result: Source[] = jsonData;
            setSources(result.map(s => ({ ...s, isSelected: true })));
        } catch (error) {
            console.error(error);
        }
    }

    const textSearch = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        await loadNews('text');
    }

    const semanticSearch = async () => {
        await loadNews('semantic');
    }

    const loadNews = async (searchType: 'semantic' | 'text' | 'none') => {
        if (searchType === 'none' && searchText.length === 0) {
            return;
        }
        try {
            setIsSearching(true);
            const suffix = searchType === 'none' ? '' : `/${searchType}?search=${searchText}`;
            const response = await fetch(`${newsURL}${suffix}`);
            const jsonData = await response.json();
            setNews(jsonData);
        } catch (error) {
            console.error(error);
        }
        setIsSearching(false);
    }

    const handleSourcesSelection = async (sourceIds: string[]) => {
        setIsSourcesVisible(false);
        setSources(sources.map(s => ({ ...s, isSelected: sourceIds.includes(s.id) })));
        await getNews(sourceIds);
    }

    useEffect(() => {
        getNews();
        getSources();
    }, []);

    return (
        <>
            <h1 className="p-4 text-2xl font-bold text-center bg-black flex justify-between">
                <a href="/"
                    className="underline text-amber-600 hover:text-amber-800 visited:text-white hidden md:inline"
                    title="Load top news"
                >
                    Space News
                </a>
                <button
                    type="button"
                    className="pl-2 pr-2 ml-3 font-semibold bg-black text-white border-2 border-white mt-2 sm:mt-0"
                    title="Select Sources"
                    onClick={() => setIsSourcesVisible(true)}
                >
                    Sources
                </button>
                <SummaryComponent />

            </h1>
            <main>
                <form
                    className="sticky top-0 bg-white m-2 relative text-gray-600 flex flex-initial justify-center"
                    onSubmit={textSearch}
                >
                    <input
                        type="search"
                        title="Content to search for"
                        className="form-input pt-1 block"
                        value={searchText}
                        onChange={(e) => setSearchText(e.target.value)}
                    />
                    <button
                        type="submit"
                        className="pl-2 pr-2 font-semibold bg-black text-white"
                        disabled={searchText.length === 0}
                        title="Run search on news"
                    >
                        Search
                    </button>
                    <button
                        type="button"
                        className="pl-2 pr-2 ml-2 font-semibold bg-white border-2 border-black text-black"
                        onClick={semanticSearch}
                        disabled={searchText.length === 0}
                        title="Run semantic search on news"
                    >
                        Semantic Search
                    </button>
                </form>
                <SourcesSelection
                    isOpen={isSourcesVisible}
                    onClose={() => setIsSourcesVisible(false)}
                    onSelect={handleSourcesSelection}
                    sources={sources}
                />
                <Loader isLoading={isSearching} />
                {!isSearching && <NewList data={news} />}
            </main>
        </>
    );
}