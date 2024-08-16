import { NewsEntry } from "./data";
import parse from 'html-react-parser';

const printDate = (d: string) => new Date(d).toLocaleString();

export const NewList: React.FC<{ data: NewsEntry[] }> = ({ data }) =>
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