
import React from 'react';
import '../../styles/LoadingSpinner.css';

const LoadingSpinner: React.FC = () => {
    return (
        <div className="loader-overlay">
            <div className="loader"></div>
        </div>
    );
};

export default LoadingSpinner;
