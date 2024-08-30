import { useState } from "react";
import { Summary } from "./data";

const apiURL = import.meta.env.VITE_API;
const summaryURL = `${apiURL}/summary`;

export const SummaryComponent: React.FC = () => {
    const [isVisible, setIsVisible] = useState<boolean>(false);
    const [summary, setSummary] = useState<string>("");
    const [playerStatus, setPlayerStatus] = useState<'none' | 'playing' | 'paused'>('none');

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
        setPlayerStatus('none');
        setIsVisible(true);
    }

    const close = () => {
        setIsVisible(false);
        speechSynthesis.cancel()
    }

    const play = () => {
        if (playerStatus === 'paused') {
            speechSynthesis.resume()
        } else if (playerStatus === 'none') {
            const utterance = new SpeechSynthesisUtterance(summary);
            const voices = speechSynthesis.getVoices();
            utterance.voice = voices[1]; // Choose a specific voice            
            speechSynthesis.speak(utterance);
            utterance.onend = () => {
                setPlayerStatus('none');
            }
        }
        setPlayerStatus('playing');
    }

    const pause = () => {
        speechSynthesis.pause();
        setPlayerStatus('paused');
    }

    const canPlay = () => {
        return summary !== '' && summary.trim().length !== 0 && playerStatus !== 'playing';
    }

    const canPause = () => {
        return summary !== '' && summary.trim().length !== 0 && playerStatus === 'playing';
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
                                        onClick={close}
                                    >
                                        X
                                    </button>
                                </h3>

                            </div>
                            <div className="pl-4 font-normal p-4">
                                <div className="flex justify-between mr-4">
                                    <button
                                        type="button"
                                        className="pl-5 pr-5 pt-2 pb-2 font-semibold bg-white border-2 border-black disabled:border-slate-300 disabled:text-slate-300"
                                        onClick={() => play()}
                                        disabled={!canPlay()}
                                    >
                                        Play
                                    </button>

                                    <button
                                        type="button"
                                        className="pl-5 pr-5 pt-2 pb-2 font-semibold bg-white border-2 border-black disabled:border-slate-300 disabled:text-slate-300"
                                        onClick={() => pause()}
                                        disabled={!canPause()}
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