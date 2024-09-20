
using StudySpace.Data.Models;
using StudySpace.Data.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Data.UnitOfWork
{
    public class UnitOfWork
    {
        public EXE201_StudySpaceContext _unitOfWorkContext;
        private AccountRepository _account;
        private AmityRepository _amityRepository;
        private BookingRepository _bookingRepository;
        private FeedbackRepository _feedbackRepository;
        private ImageFeedbackRepository _imageFeedbackRepository;
        private ImageRoomRepository _imageRoomRepository;
        private PackageRepository _package;
        private RoomRepository _roomRepository;
        private SpaceRepository _spaceRepository;
        private StorePackageRepository _storePackageRepository;
        private StoreRepository _storeRepository;
        private TransactionRepository _transactionRepository;
        private UserRoleRepository _userRoleRepository;

        public UnitOfWork()
        {
            _unitOfWorkContext ??= new EXE201_StudySpaceContext();
        }
        public UnitOfWork(EXE201_StudySpaceContext unitOfWorkContext)
        {
            _unitOfWorkContext ??= unitOfWorkContext;
        }
        public AccountRepository AccountRepository
        {
            get
            {
                return _account ??= new AccountRepository(_unitOfWorkContext);
            }
        }

        public AmityRepository AmityRepository
        {
            get
            {
                return _amityRepository ??= new AmityRepository(_unitOfWorkContext);
            }
        }

        public BookingRepository BookingRepository
        {
            get
            {
                return _bookingRepository ??= new BookingRepository(_unitOfWorkContext);
            }
        }

        public FeedbackRepository FeedbackRepository
        {
            get
            {
                return _feedbackRepository ??= new FeedbackRepository(_unitOfWorkContext);
            }
        }

        public ImageFeedbackRepository ImageFeedbackRepository
        {
            get
            {
                return _imageFeedbackRepository ??= new ImageFeedbackRepository(_unitOfWorkContext);
            }
        }

        public ImageRoomRepository ImageRoomRepository
        {
            get
            {
                return _imageRoomRepository ??= new ImageRoomRepository(_unitOfWorkContext);
            }
        }

        public PackageRepository PackageRepository
        {
            get
            {
                return _package ??= new PackageRepository(_unitOfWorkContext);
            }
        }        

        public RoomRepository RoomRepository
        {
            get
            {
                return _roomRepository ??= new RoomRepository(_unitOfWorkContext);
            }
        }

        public SpaceRepository SpaceRepository
        {
            get
            {
                return _spaceRepository ??= new SpaceRepository(_unitOfWorkContext);    
            }
        }

        public StorePackageRepository StorePackageRepository
        {
            get
            {
                return _storePackageRepository ??= new StorePackageRepository(_unitOfWorkContext);
            }
        }


        public StoreRepository StoreRepository
        {
            get
            {
                return _storeRepository ??= new StoreRepository(_unitOfWorkContext);
            }
        }

        public TransactionRepository TransactionRepository
        {
            get
            {
                return _transactionRepository ??= new TransactionRepository(_unitOfWorkContext);
            }
        }

        public UserRoleRepository UserRoleRepository
        {
            get
            {
                return _userRoleRepository ??= new UserRoleRepository(_unitOfWorkContext);
            }
        }

        ////TO-DO CODE HERE/////////////////

        #region Set transaction isolation levels

        /*
        Read Uncommitted: The lowest level of isolation, allows transactions to read uncommitted data from other transactions. This can lead to dirty reads and other issues.

        Read Committed: Transactions can only read data that has been committed by other transactions. This level avoids dirty reads but can still experience other isolation problems.

        Repeatable Read: Transactions can only read data that was committed before their execution, and all reads are repeatable. This prevents dirty reads and non-repeatable reads, but may still experience phantom reads.

        Serializable: The highest level of isolation, ensuring that transactions are completely isolated from one another. This can lead to increased lock contention, potentially hurting performance.

        Snapshot: This isolation level uses row versioning to avoid locks, providing consistency without impeding concurrency. 
         */

        public int SaveChangesWithTransaction()
        {
            int result = -1;

            //System.Data.IsolationLevel.Snapshot
            using (var dbContextTransaction = _unitOfWorkContext.Database.BeginTransaction())
            {
                try
                {
                    result = _unitOfWorkContext.SaveChanges();
                    dbContextTransaction.Commit();
                }
                catch (Exception)
                {
                    //Log Exception Handling message                      
                    result = -1;
                    dbContextTransaction.Rollback();
                }
            }

            return result;
        }

        public async Task<int> SaveChangesWithTransactionAsync()
        {
            int result = -1;

            //System.Data.IsolationLevel.Snapshot
            using (var dbContextTransaction = _unitOfWorkContext.Database.BeginTransaction())
            {
                try
                {
                    result = await _unitOfWorkContext.SaveChangesAsync();
                    dbContextTransaction.Commit();
                }
                catch (Exception)
                {
                    //Log Exception Handling message                      
                    result = -1;
                    dbContextTransaction.Rollback();
                }
            }

            return result;
        }
        #endregion

    }
}
