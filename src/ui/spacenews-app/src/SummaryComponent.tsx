import { useState } from "react";
import { Summary } from "./data";

const apiURL = import.meta.env.VITE_API;
const summaryURL = `${apiURL}/summary`;

export const SummaryComponent: React.FC = () => {
    const [isVisible, setIsVisible] = useState<boolean>(false);
    const [summary, setSummary] = useState<string>("");
    const [wasPlaying, setWasPlaying] = useState<boolean>(false);
    const [isPlaying, setIsPlaying] = useState<boolean>(false);

    const loadSummary = async () => {
        try {
            const response = await fetch(summaryURL);
            const jsonData = await response.json();
            const result: Summary = jsonData;
            setSummary(result.summary);
        } catch (error) {
            console.error(error);
        }
    }

    const displaySummary = async () => {
        if (summary === "") {
            await loadSummary();
        }
        setIsVisible(true);
    }

    const play = () => {
        if (isPlaying) {
            return;
        }
        setWasPlaying(true);
        setIsPlaying(true);
        if (wasPlaying) {
            speechSynthesis.resume()
            
        } else {
            const utterance = new SpeechSynthesisUtterance(summary);
            const voices = speechSynthesis.getVoices();
            utterance.voice = voices[0]; // Choose a specific voice            
            speechSynthesis.speak(utterance);
        }
    }

    const pause = () => {
        speechSynthesis.pause();
        setIsPlaying(false);
    }

    return (
        <>
            <button
                type="button"
                className="pl-2 pr-2 ml-3 font-semibold bg-black text-white border-2 border-white mt-2 sm:mt-0"
                onClick={displaySummary}
            >
                Today's summary
            </button>
            {isVisible && <div className="relative z-10" aria-labelledby="modal-title" role="dialog" aria-modal="true">

                <div className="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity" aria-hidden="true"></div>

                <div className="fixed inset-0 z-10 w-screen overflow-y-auto">
                    <div className="flex items-end justify-center p-4 text-center sm:items-center sm:p-0">

                        <div className="relative transform overflow-hidden rounded-lg bg-white text-left shadow-xl transition-all sm:my-8 sm:w-full sm:max-w-lg">
                            <div className="bg-black px-4 pb-4 pt-5 sm:p-6 sm:pb-4">


                                <h3
                                    className="text-base font-semibold leading-6 text-white flex justify-between items-center"
                                    id="modal-title"
                                >
                                    Today's summary
                                    <button
                                        type="button"
                                        className="ml-5 pl-5 pr-5 pt-3 pb-3 font-semibold bg-black border-2 border-white"
                                        onClick={() => setIsVisible(false)}
                                    >
                                        X
                                    </button>
                                </h3>

                            </div>
                            <div className="pl-4 font-normal p-4">
                                <div className="flex justify-between mr-4">
                                    <button
                                        type="button"
                                        className="pl-5 pr-5 pt-2 pb-2 font-semibold bg-white border-2 border-black"
                                        onClick={() => play()}
                                        disabled={summary === '' || summary.trim().length === 0 }
                                    >
                                        Play
                                    </button>

                                    <button
                                        type="button"
                                        className="pl-5 pr-5 pt-2 pb-2 font-semibold bg-white border-2 border-black"
                                        onClick={() => pause()}
                                        disabled={summary === '' || summary.trim().length === 0 || !isPlaying}
                                    >
                                        Pause
                                    </button>
                                </div>
                                <div className="overflow-y-scroll h-96 mt-2">
                                    {summary}
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            }
        </>
    );
};