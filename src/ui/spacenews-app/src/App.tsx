import { Route, Routes } from "react-router-dom"
import { SearchNewsPage } from "./pages/search-news-page"
import { IssTracketPage } from "./pages/iss-tracker-page"

function App() {
  return (
    <>
      <Routes>
        <Route path='/' element={<SearchNewsPage />} />
        <Route path='/news' element={<SearchNewsPage />} />
        <Route path='/iss' element={<IssTracketPage />} />
      </Routes> 
    </>
  )
}

export default App
