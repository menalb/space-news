import { useEffect, useState } from "react";
import parse from 'html-react-parser';

const apiURL = import.meta.env.VITE_API;

type resultData = {
  title: string;
  description: string;
  source: string;
  publishDate: string;
  links: {
    title: string,
    uri: string
  }[]
};

function App() {
  const [data, setData] = useState<resultData[]>([]);
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

  const search = async (e: React.FormEvent<HTMLFormElement>) => {
    try {
      setIsSearching(true);
      e.preventDefault();
      if (searchText.length > 0) {
        const response = await fetch(`${apiURL}/semantic?search=${searchText}`);
        const jsonData = await response.json();
        setData(jsonData);
      }
    } catch (error) {
      console.error(error);
    }
    setIsSearching(false);
  }

  useEffect(() => {
    getNews();
  }, [])

  const printDate = (d: string) => {
    return new Date(d).toLocaleString();
  }

  return (
    <>
      <h1 className="p-4 text-2xl font-bold text-center bg-black text-amber-600">SpaceNews</h1>
      <main>
        <form
          className="m-2 relative text-gray-600 flex flex-initial justify-center"
          onSubmit={search}
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
          >
            Search
          </button>
        </form>
        {isSearching &&
          <>
            <div className="flex justify-center h-screen mt-20 h-10">
              <div className="relative inline-flex justify-items-start">
                <div className="w-8 h-8 bg-amber-600 rounded-full"></div>
                <div className="w-8 h-8 bg-amber-600 rounded-full absolute top-0 left-0 animate-ping"></div>
                <div className="w-8 h-8 bg-amber-600 rounded-full absolute top-0 left-0 animate-pulse"></div>
              </div>
            </div>
          </>
        }
        {!isSearching &&
          <ul>
            {data.map((value, index) => {
              return (
                <li key={`${index}-${value.title}`} className="flex justify-start flex-col p-2">
                  <span>
                    <b>{value.title}</b>
                  </span>
                  <span>
                    {printDate(value.publishDate)}
                  </span>
                  <span>
                    {parse(value.description)}
                  </span>
                  <span>
                    <em>{value.source}</em>
                  </span>
                  <span>
                    {value.links.map(link => (
                      <span className="pr-1">
                        [
                        <a
                          target="blank"
                          key={link.title}
                          href={link.uri}
                          className="pl-1 pr-1"
                          title="Open news"
                        >
                          {link.title ?? "Open"}
                        </a>
                        ]
                      </span>
                    ))}
                  </span>
                </li>
              );
            })}
          </ul>
        }
      </main>
    </>
  )
}

export default App
