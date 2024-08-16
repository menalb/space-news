import { useEffect, useState } from "react";
import { NewsEntry } from "./data";
import { NewList } from "./NewsList";
import { Loader } from "./Loader";

const apiURL = import.meta.env.VITE_API;

function App() {
  const [data, setData] = useState<NewsEntry[]>([]);
  const [isSearching, setIsSearching] = useState<boolean>(false);
  const [searchText, setSearchText] = useState<string>("");

  const getNews = async () => {
    try {
      setIsSearching(true);
      const response = await fetch(`${apiURL}`);
      const jsonData = await response.json();
      setData(jsonData);
    } catch (error) {
      console.error(error);
    }
    setIsSearching(false);
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
      const response = await fetch(`${apiURL}${suffix}`);
      const jsonData = await response.json();
      setData(jsonData);
    } catch (error) {
      console.error(error);
    }
    setIsSearching(false);
  }

  useEffect(() => {
    getNews();
  }, [])

  return (
    <>
      <h1 className="p-4 text-2xl font-bold text-center bg-black">
        <a href="/"
          className="underline text-amber-600 hover:text-amber-800 visited:text-amber-600"
          title="Load top news"
        >
          Space News
        </a>
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
            className="pl-2 pr-2 font-semibold bg-black text-amber-600"
            disabled={searchText.length === 0}
            title="Run search on news"
          >
            Search
          </button>
          <button
            type="button"
            className="pl-2 pr-2 ml-2 font-semibold bg-white border-2 border-black text-amber-600"
            onClick={semanticSearch}
            disabled={searchText.length === 0}
            title="Run semantic search on news"
          >
            Try Semantic Search
          </button>
        </form>
        <Loader isLoading={isSearching} />
        {!isSearching && <NewList data={data} />}
      </main>
    </>
  )
}

export default App
