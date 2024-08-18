import { useEffect, useState } from "react";
import { Source } from "./data";

interface Props {
    isOpen: boolean;
    onClose: () => void;
    onSelect: (sourcesId: string[]) => void;
    sources: Source[]
}

export const SourcesSelection: React.FC<Props> = ({
    isOpen,
    onClose,
    onSelect,
    sources
}) => {
    const [isVisible, setIsVisible] = useState<boolean>(false);
    const [data, setData] = useState<Source[]>(sources);
    const [checkedSources, setCheckedSources] = useState<string[]>([]);

    useEffect(() => {
        setIsVisible(isOpen);
    }, [isOpen])

    useEffect(() => {
        setData(sources);
        setCheckedSources(sources.filter(s => s.isSelected).map(s => s.id));
    }, [sources])

    const handleClose = () => {
        setIsVisible(false);
        onClose();
    }

    const handleSelect = () => {
        setIsVisible(false);
        onSelect(checkedSources);
    }

    const handleCheck = (id: string, value: boolean) => {
        if (value === false) {
            setCheckedSources(checkedSources.filter(s => s !== id));
        } else {
            setCheckedSources([...checkedSources, id]);
        }
    }

    return (isVisible &&
        <div className="relative z-10" aria-labelledby="modal-title" role="dialog" aria-modal="true">

            <div className="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity" aria-hidden="true"></div>

            <div className="fixed inset-0 z-10 w-screen overflow-y-auto">
                <div className="flex items-end justify-center p-4 text-center sm:items-center sm:p-0">

                    <div className="relative transform overflow-hidden rounded-lg bg-white text-left shadow-xl transition-all sm:my-8 sm:w-full sm:max-w-lg">
                        <div className="bg-black px-4 pb-4 pt-5 sm:p-6 sm:pb-4">
                            <div className="sm:flex sm:items-start">
                                <div className="mt-3 text-center sm:ml-4 sm:mt-0 sm:text-left">
                                    <h3
                                        className="text-base font-semibold leading-6 text-white"
                                        id="modal-title"
                                    >
                                        Select Sources
                                    </h3>
                                </div>
                            </div>
                        </div>
                        {data && <ul className="pl-4 mt-2">
                            {data.map((s) => <li
                                key={s.id}
                                className="pl-4 pt-2 pb-2"
                            >
                                <CheckBox
                                    source={s}
                                    onChecked={handleCheck}
                                />
                            </li>)}
                        </ul>}
                        <div className="bg-gray-50 px-4 py-3 text-center mt-5 md:mt-2 flex justify-center">
                            <button
                                type="button"
                                className="mr-5 pl-5 pr-5 pt-3 pb-3 font-semibold bg-black border-2 border-black text-white"
                                onClick={handleSelect}
                            >
                                Select
                            </button>
                            <button
                                type="button"
                                className="ml-5 pl-5 pr-5 pt-3 pb-3 font-semibold bg-white border-2 border-black"
                                onClick={handleClose}
                            >
                                Cancel
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}

const CheckBox = (props: { source: Source, onChecked: (id: string, value: boolean) => void }) => {
    const s = props.source;
    const [isChecked, setIsChecked] = useState(props.source.isSelected);

    const handleOnChange = () => {
        setIsChecked(!isChecked);
        props.onChecked(s.id, !isChecked);
    };

    return (
        <>
            <label htmlFor={s.id}>
                <span className="group flex rounded ring-black active:ring-1">
                    <input
                        id={s.id}
                        type="checkbox"
                        className={`h-8 w-8 cursor-pointer rounded-full border-back bg-white text-black focus:ring-black`}
                        checked={isChecked}
                        onChange={handleOnChange}
                        value={s.id}
                    />
                    <p className="text-reg cursor-pointer pl-2 decoration-solid group-hover:underline">
                        {s.name}
                        <span>{s.isSelected ? "A" : "B"}</span>
                    </p>
                </span>
            </label>
        </>
    )
}

