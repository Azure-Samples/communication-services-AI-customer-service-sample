import React, { useEffect, useState } from 'react';
import Modal from 'react-modal';
import { FinalSummary, getCustomerCommunicationDetails, SendSummaryDetails, SummaryEmailData } from '../../utils/SendSummaryDetails';
import { getEmailTemplate } from '../../utils/SummaryEmailTemplate';
import { ConversationalInsights, getSummaryDetails } from '../../utils/SummaryList';
import '../../styles/SummaryWindow.css';
import LoadingSpinner from '../LoadingSpinner/LoadingSpinner';

type TaskType = {
    id: number;
    description: string;
};

const tasks: TaskType[] = [
    { id: 1, description: 'Schedule on site assessment' },
    { id: 2, description: 'Assign field technician' },
    { id: 3, description: 'Verify customer information' },
];


const SummaryWindow: React.FC = () => {
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [isLoading, setIsLoading] = useState(false);
    const [summaryDetails, setSummaryDetails] = useState<ConversationalInsights>();
    const [summaryConversation, setSummaryConversation] = useState<FinalSummary>()
    const chatThreadId = localStorage.getItem("chatThreadId");
    useEffect(() => {
        if (isModalOpen) {
            setIsLoading(true);
            getSummaryDetails(chatThreadId).then(summaryData => {
                setSummaryDetails(summaryData);
                setIsLoading(false);
            }).catch((error) => {
                setIsLoading(false);
                throw new Error(error)
            });;
            getCustomerCommunicationDetails(chatThreadId)
                .then(apiData => {
                    setSummaryConversation(apiData);
                }).catch((error) => {
                    setIsLoading(false);
                    throw new Error(error)
                });
        }

    }, [chatThreadId, isModalOpen]);

    const handleSendSummary = async () => {
        const emailBodyTemplate = await getEmailTemplate(summaryConversation?.result);
        const email: SummaryEmailData = {
            body: emailBodyTemplate
        };
        const response = await SendSummaryDetails(email)
        if (response.status === "Succeeded") {
            alert("Email sent successfully")

        } else {
            alert("Email failed.")
        }

    };

    return (
        <div>
            <div className="send-summary-container">
                <button className='send-summary-button' onClick={() => setIsModalOpen(true)}>Send Summary</button>
            </div>
            <Modal
                isOpen={isModalOpen}
                onRequestClose={() => setIsModalOpen(false)}
                className="dialog"
                overlayClassName="overlay"
                ariaHideApp={false}
            >
                <div>{isLoading && <LoadingSpinner />}</div>
                <div className="titlebar">Case Summary</div>
                <section className="summary-section">
                    <h2>Summary</h2>
                    <p className="bordered-content">{summaryDetails?.summaryItems[0].description}</p>
                </section>
                <section className="tasks-section">
                    <h2>Tasks</h2>
                    <ul>
                        {tasks.map(task => (
                            <li key={task.id}>
                                <input type="checkbox" id={`task-${task.id}`} />
                                <label htmlFor={`task-${task.id}`}>{task.description}</label>
                            </li>
                        ))}
                    </ul>
                </section>
                <section className="communication-section">
                    <h2>Customer Communication</h2>
                    <p className="customer-communication">
                        {summaryConversation?.result}
                    </p>
                </section>
                <div className="button-group">
                    <button className="action-button send-summary-button" onClick={handleSendSummary}>Send Summary</button>
                </div>
            </Modal>
        </div>
    );
};

export default SummaryWindow;
